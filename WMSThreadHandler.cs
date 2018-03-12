using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace WMSServer
{
    enum ServiceOperations
    {
        GetCapabilities,
        GetMap
    }

    class WMSThreadHandler
    {

        private TcpListener wmsListener;
        private string requestData;  //请求字符串
        private string sMimeType;    //请求格式
        private MapRequest mapReq;   //GetMap请求
        private CapabilityRequest capalitityReq;
        private ServiceOperations serveType = ServiceOperations.GetCapabilities;
        public WMSThreadHandler(TcpListener mytcpListener)
        {
            this.wmsListener = mytcpListener;
        }

        public void HandleThread()
        {
            TcpClient client = wmsListener.AcceptTcpClient();
            NetworkStream stream = client.GetStream();
            try
            {
                GetRequest(ref stream);
                SendResponse(ref stream, GetResponceData());
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(WMSExpection))
                {
                    Byte[] errordata = Encoding.UTF8.GetBytes(WMSServer.Properties.Resources.errorPagePart1 + ex.Message + WMSServer.Properties.Resources.errorPagePart2);
                    sMimeType = "";
                    SendResponse(ref stream, errordata);
                }
                Console.WriteLine(ex.Message);
                return;
            }
        }

        private bool GetRequest(ref NetworkStream stream)
        {
            Byte[] data = new Byte[1024];
            requestData = String.Empty;
            Int32 bytes = stream.Read(data, 0, data.Length);
            requestData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
            Console.WriteLine(requestData);
            try
            {
                if (requestData.IndexOf("Capabilities") != -1)  //请求是GetCapabilities
                {
                    capalitityReq = new CapabilityRequest(requestData);
                    serveType = ServiceOperations.GetCapabilities;
                    sMimeType = "text/xml";
                }
                else  //请求是GetMap
                {
                    mapReq = new MapRequest(requestData);
                    serveType = ServiceOperations.GetMap;
                    sMimeType = mapReq.FORMAT;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return true;
        }

        private Byte[] GetResponceData()
        {
            if (serveType == ServiceOperations.GetCapabilities)
                return WMS.GetCapabilityData(capalitityReq);
            else if (serveType == ServiceOperations.GetMap)
                return WMS.GetMap(mapReq);
            return null;
        }

        private void SendResponse(ref NetworkStream stream, Byte[] responseData)
        {
            int iTotBytes = responseData.Length;
            string sHttpVersion = requestData.Substring(requestData.IndexOf("HTTP", 1), 8);
            SendHeader(sHttpVersion, sMimeType, iTotBytes, " 200 OK", ref stream);
            SendToBrowser(responseData, ref stream);
        }

        private void SendHeader(string sHttpVersion, string sMIMEHeader, int iTotBytes, string sStatusCode, ref NetworkStream mySocket)
        {
            String sBuffer = "";
            if (sMIMEHeader.Length == 0)
                sMIMEHeader = "text/html";
            sBuffer = sBuffer + sHttpVersion + sStatusCode + "\r\n";
            sBuffer = sBuffer + "Server: WMSServer\r\n";
            sBuffer = sBuffer + "Content-Type: " + sMIMEHeader + "\r\n";
            sBuffer = sBuffer + "Accept-Ranges: bytes\r\n";
            sBuffer = sBuffer + "Content-Length: " + iTotBytes + "\r\n\r\n";
            Byte[] bSendData = Encoding.ASCII.GetBytes(sBuffer);
            Console.Write(sBuffer);
            SendToBrowser(bSendData, ref mySocket);
        }

        private void SendToBrowser(Byte[] bSendData, ref NetworkStream mySocket)
        {
            try
            {
                if (mySocket.CanWrite)
                    mySocket.Write(bSendData, 0, bSendData.Length);
                Console.WriteLine("资源大小：" + bSendData.Length.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("发生错误 : {0} ", e);
            }
        }
    }
}
