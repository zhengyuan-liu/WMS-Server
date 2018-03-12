using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMSServer
{
    class Program
    {
        static void Main(string[] args)
        {
            WMSListener WMSServer = new WMSListener();
            WMSServer.Start();
        }
    }
}
