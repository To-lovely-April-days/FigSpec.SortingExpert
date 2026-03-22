using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FigSpec.SortingExpert.Tools
{
    public class FileTool
    {
        /// <summary>
        /// 删除文件夹中所有文件
        /// </summary>
        /// <param name="folderPath"></param>
        public static void DeleteAllFiles(string folderPath, string extension)
        {
            //Thread.Sleep(5);
            // 获取文件夹中的所有文件
            string[] files = Directory.GetFiles(folderPath);

            // 遍历文件并删除
            foreach (string file in files)
            {
                if(file.EndsWith(extension))
                    File.Delete(file);
                //Console.WriteLine($"Deleted {file}");
            }

            // 获取文件夹中的所有子文件夹
            string[] subDirs = Directory.GetDirectories(folderPath);

            // 递归地删除子文件夹中的文件
            foreach (string subDir in subDirs)
            {
                DeleteAllFiles(subDir, extension);
            }
        }

        public static void OpenPdf(string pdfPath)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = pdfPath,
                UseShellExecute = true
            });
        }
    }
}
