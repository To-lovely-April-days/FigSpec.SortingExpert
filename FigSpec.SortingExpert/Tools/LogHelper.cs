using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FigSpec.SortingExpert.Tools
{
    public class LogHelper
    {
        private static readonly log4net.ILog loginfo = log4net.LogManager.GetLogger("loginfo");//这里的 loginfo 和 log4net.config 里的名字要一样
        private static readonly log4net.ILog logerror = log4net.LogManager.GetLogger("logerror");//这里的 logerror 和 log4net.config 里的名字要一样

        public static void WriteLog(string info)
        {
            if (loginfo.IsInfoEnabled)
            {
                loginfo.Info(info);
            }
        }

        public static void WriteLog(string info, Exception ex)
        {
            if (logerror.IsErrorEnabled)
            {
                logerror.Error(info, ex);
            }
        }


        private static StringBuilder logMessageBuilder = new StringBuilder();

        public static void WriteLine(long mes)
        {
            WriteLine(mes.ToString());
        }

        public static void WriteLine(string mes)
        {
            Console.WriteLine(mes);
            //logMessageBuilder.AppendLine(mes);
        }

        public static void AppendToFile()
        {
            return;
            string filePath = Path.Combine(string.Format(GlobalSettings.ApplyInfo.ApplyMyDocuments, "log"), "DebugLog.txt");
            File.AppendAllText(filePath, logMessageBuilder.ToString());
            logMessageBuilder.Clear();
        }
    }
}