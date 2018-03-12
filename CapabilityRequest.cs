using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace WMSServer
{
    class CapabilityRequest
    {
        private string service = "WMS";
        private string request = "GetCapabilities";
        private string[] Versions = null;
        private string updataSequence = "";
        private string[] acceptFormats = null;
        private string requestData;

        public CapabilityRequest(string requestData)
        {
            Console.WriteLine(requestData);
            string temp = Regex.Match(requestData.Split('\n')[0], @"/.+?HTTP").Value;
            Console.WriteLine(temp);
            string paras = temp.Substring(1, temp.Length - 5);
            SetPara(paras);
        }

        private void SetPara(string paras)
        {
            try
            {
                string[] msgs = paras.Split(@"&?".ToArray(), StringSplitOptions.RemoveEmptyEntries);
                int i = 0, n = msgs.Length;
                for (i = 1; i < n; i++)
                    SetParameter(msgs[i]);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SetParameter(string msg)
        {
            try
            {
                string[] paras = msg.Split("= ".ToArray(), StringSplitOptions.RemoveEmptyEntries);
                if (paras[0] == "Version")
                    paras.CopyTo(Versions, 1);
                else if (paras[0] == "updateSequence")
                    updataSequence = paras[1];
                else if (paras[0] == "Format")
                    paras.CopyTo(acceptFormats, 1);
            }
            catch (Exception)
            {
                throw new WMSExpection("操作请求包含无效参数值");
            }
        }
    }
}
