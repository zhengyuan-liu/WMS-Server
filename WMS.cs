using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using shp读取;

namespace WMSServer
{
    class WMSExpection : Exception
    {
        public WMSExpection() { }
        public WMSExpection(string message) : base(message) { }
        public WMSExpection(string message, Exception inner) : base(message, inner) { }
    }

    class WMS
    {
        public static Byte[] GetMap(MapRequest mapReq)
        {
            string filepath = @"D:\MyDocuments\pkumap\";
            Bitmap tempbmp = new Bitmap(mapReq.Width, mapReq.Height);
            for (int i = 0; i < mapReq.layers.Length; i++)
            {
                string filename = filepath + mapReq.layers[i] + ".shp";
                Shapefile shp = new Shapefile(filename);
                FeatureClass fc = shp.GetFeature();
                tempbmp = fc.DrawBMP(mapReq.bbox, tempbmp, mapReq.styles[0]);
            }

            MemoryStream ms = new MemoryStream();
            tempbmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            byte[] bitmapData = new Byte[ms.Length];
            bitmapData = ms.ToArray();
            return bitmapData;
        }

        public static Byte[] GetCapabilityData(CapabilityRequest req)
        {
            return Encoding.UTF8.GetBytes(WMSServer.Properties.Resources.capability);
        }
    }
}
