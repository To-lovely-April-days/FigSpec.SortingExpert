using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace FigSpec.SortingExpert.Tools
{
    public static class PythonHelper
    {
        public static async Task<string[]> Command(string inputPath, Dictionary<string, string> args, CancellationToken cancellationToken)
        {
            string argumentsStr = string.Empty; 
            string argStr = $" {args.ToArgsStr()}";
            argumentsStr = inputPath + " " + argStr ;
            Process process = new Process();
            string ImagePath = @"\pls\pls.exe";
            string path = Environment.CurrentDirectory + ImagePath;
            process.StartInfo.FileName = path;
            // process.StartInfo.FileName = "D:\\Users\\ym\\Desktop\\main\\main.exe";
            process.StartInfo.Arguments = argumentsStr;
         
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.Start();

            string output = null;
            // 使用 Task 来等待进程退出
            await Task.Run(() =>
            {
                process.WaitForExit();
                if (!cancellationToken.IsCancellationRequested)
                {
                    output = process.StandardOutput.ReadToEnd().Replace("\r\n", "");
                    //Console.WriteLine("进程已正常退出。");
                }
            }, cancellationToken);
            process.Close();
            return output?.Split(' ');
        }
        public static Task Command(string action, Dictionary<string, string> args, ICommandReceived rec)
        {
            return Task.Run(() =>
            {
                string argStr = $"{action} {args.ToArgsStr()}";
                Trace.WriteLine(argStr);
                Process process = new Process();
                process.StartInfo.FileName = "stitch\\stitch.exe";
                process.StartInfo.Arguments = argStr;

                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;

                string[] res = null;
                string err = null;
                process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
                {
                    if (string.IsNullOrEmpty(e.Data)) return;
                    Trace.WriteLine($"stitch cmd: {e.Data}");
                    string[] _res = e.Data.Split(' ');
                    if (_res[0] == "OK")
                    {
                        res = _res;
                    }
                    else if (_res[0] == "PRO")
                    {
                        rec.OnCommandProgress(int.Parse(_res[1]), int.Parse(_res[2]), _res[3]);
                    }
                    else if (_res[0] == "LOG")
                    {
                        rec.OnCommandLog(e.Data.Remove(0, 4));
                    }
                    else if (_res[0] == "ERR")
                    {
                        err = e.Data.Remove(0, 4);
                    }
                };
                process.Start();
                process.BeginOutputReadLine();
                process.WaitForExit();
                if (err == null && res != null)
                {
                    rec.OnCommandOk(res);
                }
                else
                {
                    rec.OnCommandError(err);
                }
                process.Close();
            });
        }
        public static string ToArgsStr(this Dictionary<string, string> args)
        {
            string argStr = "";
            foreach (var key in args.Keys)
            {
                argStr += $"--{key} {args[key]} ";
            }
            return argStr;
        }
        public interface ICommandReceived
        {
            /// <summary>
            /// 进度更新
            /// </summary>
            /// <param name="i"></param>
            /// <param name="max"></param>
            /// <param name="msg"></param>
            void OnCommandProgress(int i, int max, string msg);

            /// <summary>
            /// 普通消息
            /// </summary>
            /// <param name="msg"></param>
            void OnCommandLog(string msg);

            /// <summary>
            /// 任务成功
            /// </summary>
            /// <param name="res"></param>
            void OnCommandOk(string[] res);

            /// <summary>
            /// 发生异常
            /// </summary>
            /// <param name="err"></param>
            void OnCommandError(string err);
        }
    }
}
