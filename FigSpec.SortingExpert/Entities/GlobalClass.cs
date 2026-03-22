using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace FigSpec.SortingExpert.Entities
{
    public class GlobalClass
    {
        /// <summary>
        /// 获取电脑的剩余内存（单位 B)
        /// </summary>
        /// <returns></returns>
        public long GetRemainingMemory()
        {
            try
            {
                // 创建一个 ManagementObjectSearcher 对象来查询 WMI  
                long availablebytes = 0;
                ManagementClass mos = new ManagementClass("Win32_OperatingSystem");
                foreach (ManagementObject mo in mos.GetInstances())
                {
                    if (mo["FreePhysicalMemory"] != null)
                    {
                        availablebytes = 1024 * long.Parse(mo["FreePhysicalMemory"].ToString());
                    }
                }
                return availablebytes;
            }
            catch (Exception ex)
            {
                // 处理任何可能出现的异常  
                Console.WriteLine("An error occurred: " + ex.Message);
                return 0;
            }
        }
    }
}
