using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using shp读取;

namespace WMSServer
{

    class MapRequest
    {
        private string service = "WMS";
        private string request = "GetMap";
        private string version = "1.3.0";
        public string[] layers;
        public string[] styles;
        private string CRS = "";
        public BBOX bbox;
        private string width = "";
        private string height = "";
        private string format = "png";

        public MapRequest(string requestData)
        {
            try
            {
                Console.WriteLine(requestData);
                string temp = Regex.Match(requestData.Split('\n')[0], @"/.+?HTTP").Value;
                string paras = temp.Substring(1, temp.Length - 5);
                SetPara(paras);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int Width
        {
            get { return Int32.Parse(this.width); }
        }

        public int Height
        {
            get { return Int32.Parse(this.height); }
        }

        public string FORMAT
        {
            get { return format; }
        }

        private void SetPara(string paras)
        {
            try
            {
                string[] msgs = paras.Split(@"?&".ToArray(), StringSplitOptions.RemoveEmptyEntries);
                int n = msgs.Length;
                for (int i = 1; i < n; i++)
                    SetParameter(msgs[i]);
                return;
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
                if (string.Equals(paras[0], "version", StringComparison.OrdinalIgnoreCase))
                    version = paras[1];
                else if (string.Equals(paras[0], "layers", StringComparison.OrdinalIgnoreCase))
                {
                    layers = paras[1].Split(',');  //只能用逗号（“，”）作为列表中各个项之间的分隔符
                }
                else if (string.Equals(paras[0], "styles", StringComparison.OrdinalIgnoreCase))
                {
                    if (paras.Length == 2)
                        styles = paras[1].Split(',');  //只能用逗号（“，”）作为列表中各个项之间的分隔符
                    else
                    {
                        styles = new string[1];
                        styles[0] = "";
                    }
                }
                else if (string.Equals(paras[0], "format", StringComparison.OrdinalIgnoreCase))
                    format = paras[1].Split('/')[1];
                else if (string.Equals(paras[0], "BBOX", StringComparison.OrdinalIgnoreCase))
                {
                    string[] datas = paras[1].Split(',');  //只能用逗号（“，”）作为列表中各个项之间的分隔符
                    bbox = new BBOX(Double.Parse(datas[0]), Double.Parse(datas[1]), Double.Parse(datas[2]), Double.Parse(datas[3]));
                }
                else if (string.Equals(paras[0], "CRS", StringComparison.OrdinalIgnoreCase))
                    CRS = paras[1];
                else if (string.Equals(paras[0], "Width", StringComparison.OrdinalIgnoreCase))
                    width = paras[1];
                else if (string.Equals(paras[0], "Height", StringComparison.OrdinalIgnoreCase))
                    height = paras[1];
            }
            catch (Exception)
            {
                throw new WMSExpection("操作请求包含无效参数值");
            }
            return;
        }
    }
}
