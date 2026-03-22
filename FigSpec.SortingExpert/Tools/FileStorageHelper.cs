using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FigSpec.SortingExpert.Tools
{
    public class FileStorageHelper
    {
        // 预先生成十六进制查找表
        private static readonly string[] HexLookup = new string[256];
        List<byte[]> collectTag = new List<byte[]>();
        
        string fileExtension = ".csv";
        string filePrefix = string.Empty;
        string fileDir = string.Empty;
        public FileStorageHelper()
        {
            for (int i = 0; i < 256; i++)
            {
                HexLookup[i] = i.ToString("X2");
            }
            string filePath = GlobalSettings.ApplySetting.CommunicationFileName;
            fileDir = Path.GetDirectoryName(filePath);
            filePrefix = Path.GetFileNameWithoutExtension(filePath);
            fileExtension = Path.GetExtension(filePath);
        }

        public void ClearData()
        {
            collectTag.Clear();
        }

        public void AddData(byte[] bytes)
        {
            collectTag.Add(bytes);
            if(collectTag.Count == GlobalSettings.ApplySetting.FrameNumberPerFile)
            {
                string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
                string filePath = Path.Combine(fileDir, $"{filePrefix}_{timestamp}{fileExtension}");

                SaveToCsv(filePath, collectTag.ToList());
                collectTag.Clear();
            }
        }

        private void SaveToCsv(string fileName, List<byte[]> tags)
        {
            try
            {
                using (var writer = new StreamWriter(fileName, false, Encoding.UTF8))
                {
                    var sb = new StringBuilder(640*5); // 预分配缓冲区
                    foreach (var byteArray in tags)
                    {
                        sb.Clear();
                        for (int i = 0; i < byteArray.Length; i++)
                        {
                            if (i > 0) sb.Append(',');
                            sb.Append(HexLookup[byteArray[i]]);
                        }

                        writer.WriteLine(sb);
                    }
                }
     
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("保存分选结果文件时出错：" + ex.Message);
                throw;
            }
        }

        // 十六进制字符查找表
        private static readonly char[] HexLookupChars = new char[]
        {
            '0', '1', '2', '3', '4', '5', '6', '7',
            '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'
        };
        private void SaveToCsv2(string fileName)
        {
            try
            {
                using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None, 65536))
                using (var writer = new StreamWriter(fileStream, Encoding.UTF8, 65536))
                {
                    byte[] newLineBytes = Encoding.UTF8.GetBytes(Environment.NewLine);
                    byte[] commaBytes = Encoding.UTF8.GetBytes(",");

                    foreach (var byteArray in collectTag)
                    {
                        for (int i = 0; i < byteArray.Length; i++)
                        {
                            if (i > 0)
                            {
                                fileStream.Write(commaBytes, 0, commaBytes.Length);
                            }

                            byte b = byteArray[i];
                            byte[] hexBytes = new byte[2]
                            {
                                (byte)(b > 0x0F ? HexLookupChars[b >> 4] : '0'),
                                (byte)HexLookupChars[b & 0x0F]
                            };

                            fileStream.Write(hexBytes, 0, 2);
                        }

                        fileStream.Write(newLineBytes, 0, newLineBytes.Length);
                    }
                }

                collectTag.Clear();
            }
            catch (Exception ex)
            {
                LogHelper.WriteLine("保存分选结果文件时出错：" + ex.Message);
                throw;
            }
        }

    }
}
