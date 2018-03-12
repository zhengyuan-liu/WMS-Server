using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;

namespace shp读取
{
    class PolygonFeature
    {
        BBOX bbox;
        int numParts;  // 当前面目标所包含的子环的个数
        int numPoints;  // 构成当前面状目标的所有顶点的个数
        int[] parts;  // 每个子环的第一个坐标点在 Points 的位置
        PointD[] points;  // 记录所有坐标点的数组
        int byteCount = 0;

        public PolygonFeature(BinaryReader br)
        {
            int recordNumber;
            int contentLength;  //坐标记录长度
            int shapeType;  //几何类型

            //读取记录头
            recordNumber = Shapefile.ReverseByte(br.ReadInt32());
            contentLength = Shapefile.ReverseByte(br.ReadInt32());
            byteCount += 8;
            //读取记录内容
            shapeType = br.ReadInt32();
            bbox.xmin = br.ReadDouble();
            bbox.ymin = br.ReadDouble();
            bbox.xmax = br.ReadDouble();
            bbox.ymax = br.ReadDouble();
            numParts = br.ReadInt32();
            numPoints = br.ReadInt32();
            byteCount += 44;
            parts = new int[numParts];
            for (int i = 0; i < numParts; i++)
                parts[i] = br.ReadInt32();
            byteCount += 4 * numParts;
            points = new PointD[numPoints];
            double x, y;
            for (int i = 0; i < numPoints; i++)
            {
                x = br.ReadDouble();
                y = br.ReadDouble();
                points[i] = new PointD(x, y);
            }
            byteCount += 16 * numPoints;
        }

        /// <summary>
        /// 字节数
        /// </summary>
        public int ByteCount
        {
            get { return this.byteCount; }
        }

        public Point[] GetBMPPoints(BBOX boundarybox, int width, int height)
        {
            Point[] bmpPoints = new Point[numPoints];
            for (int i = 0; i < numPoints; i++)
            {
                double x = width * (points[i].X - boundarybox.xmin) / (boundarybox.xmax - boundarybox.xmin);
                double y = height * (points[i].Y - boundarybox.ymin) / (boundarybox.ymax - boundarybox.ymin);
                bmpPoints[i] = new Point((int)x, height - (int)y);
            }
            return bmpPoints;
        }
    }
}
