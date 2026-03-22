using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FigSpec.SortingExpert.Tools
{
    public interface IBaseEjectDevice
    {
         bool OpenByTcp(string IP, int port);
         bool OpenByCom(string COM, int AirBaudRate = 115200);
         bool Close();
         bool Eject(int[] ports, ushort time);
         bool Set(int[] devices,int[] data);
         bool IsConnected { get;  }
    }
}
