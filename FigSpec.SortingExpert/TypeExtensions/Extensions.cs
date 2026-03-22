using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FigSpec.SortingExpert.TypeExtensions
{
    public static class StringExtensions
    {
        public static string ChangeExtension(this string input, string ext)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            string fileName = Path.GetFileNameWithoutExtension(input); // 获取文件名（无后缀）
            string directory = Path.GetDirectoryName(input); // 获取文件所在目录
            string newFileName = fileName + ext; // 构建新文件名

            return Path.Combine(directory, newFileName); // 返回新的文件路径
        }

        public static bool IsValidPath(this string path)
        {
            try
            {
                Path.GetFullPath(path);
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (PathTooLongException)
            {
                return false;
            }
            catch (NotSupportedException)
            {
                return false;
            }
        }
    }
}
