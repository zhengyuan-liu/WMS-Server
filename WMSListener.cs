using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace WMSServer
{
    class WMSListener
    {
         private TcpListener myTcpListener;

        public WMSListener()
        {
            myTcpListener = new TcpListener(IPAddress.Any, 8080);
        }

        public void Start()
        {
            myTcpListener.Start();
            Console.WriteLine("WMS Server started. press ctrl+C to stop\n");
            while (true)
            {
                while (!myTcpListener.Pending()) ;
                WMSThreadHandler myWorker = new WMSThreadHandler(this.myTcpListener);
                Thread myWorkerthread = new Thread(new ThreadStart(myWorker.HandleThread));
                myWorkerthread.Name = "Created at" + DateTime.Now.ToString();
                myWorkerthread.Start();
            }
        }
    }
}
