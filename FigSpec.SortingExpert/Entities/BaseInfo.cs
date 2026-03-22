using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FigSpec.SortingExpert.Entities
{
    public class BaseInfo
    {
        public string uid { get; set; } = Guid.NewGuid().ToString();
        public string name { get; set; }
        public string createTime { get; set; } = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        public string version { get; set; } = "V1.0.1";
        public string remark { get; set; }
    }
}
