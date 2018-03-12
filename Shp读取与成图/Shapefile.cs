using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace shp读取
{
    enum ShapeTypes
    {
        NullShape, Point, Polyline, Polygon, MultiPoint, PointZ, PolyLineZ, PolygonZ, MultiPointZ, PointM, PolyLineM, PolygonM, MultiPointM, MultiPatch
    }

    class Shapefile
    {       
        int fileCode;
        int fileLength;  //文件长度，单位：字节
        int version;  //版本号
        int shapeTypeID;  //集合类型
        ShapeTypes shapeType;
        double xmin, ymin, xmax, ymax, zmin, zmax, mmin, mmax;
        int byteCount = 0;  //已经读取的字节数
        FeatureClass feature;

        public Shapefile(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Open);
            BinaryReader br = new BinaryReader(fs);

            //读取文件头
            fileCode = ReverseByte(br.ReadInt32());
            for (int i = 1; i <= 5; i++)
                br.ReadInt32();
            fileLength = ReverseByte(br.ReadInt32()) * 2;
            version = br.ReadInt32();
            shapeTypeID = br.ReadInt32();
            xmin = br.ReadDouble();
            ymin = br.ReadDouble();
            xmax = br.ReadDouble();
            ymax = br.ReadDouble();
            zmin = br.ReadDouble();
            zmax = br.ReadDouble();
            mmin = br.ReadDouble();
            mmax = br.ReadDouble();
            byteCount += 100;

            switch (shapeTypeID)
            {
                case 1:
                    {
                        shapeType = ShapeTypes.Point;
                        feature = ReadPointShp(br);
                        break;
                    }
                case 3:
                    {
                        shapeType = ShapeTypes.Polyline;
                        feature = ReadPolyLineShp(br);
                        break;
                    }
                case 5:
                    {
                        shapeType = ShapeTypes.Polygon;
                        feature = ReadPolygonShp(br);
                        break;
                    }
                default:
                    {
                        shapeType = ShapeTypes.NullShape;
                        feature = new FeatureClass();
                        break;
                    }
            }
        }

        /// <summary>
        /// 返回shapefile所对应的要素类
        /// </summary>
        /// <returns>要素类</returns>
        public FeatureClass GetFeature()
        {
            return feature;
        }

        /// <summary>
        /// 大尾整数转小尾整数
        /// </summary>
        /// <param name="big">大尾整数</param>
        /// <returns>小尾整数</returns>
        public static int ReverseByte(int big)
        {
            byte[] bytes = BitConverter.GetBytes(big);
            ExchangeByte(ref bytes[0], ref bytes[3]);
            ExchangeByte(ref bytes[1], ref bytes[2]);
            int little = BitConverter.ToInt32(bytes, 0);
            return little;
        }

        public static void ExchangeByte(ref byte b1, ref byte b2)
        {
            byte temp;
            temp = b1;
            b1 = b2;
            b2 = temp;
        }

        FeatureClass ReadPointShp(BinaryReader br)
        {
            List<PointFeature> points = new List<PointFeature>();
            int recordNumber;  //记录号
            int contentLength;  //坐标记录长度
            int shapeType;  //几何类型
            double x, y;
            while (byteCount < fileLength)
            {
                recordNumber = ReverseByte(br.ReadInt32());
                contentLength = ReverseByte(br.ReadInt32());
                shapeType = br.ReadInt32();
                x = br.ReadDouble();
                y = br.ReadDouble();
                PointFeature p = new PointFeature(x, y);
                points.Add(p);
                byteCount += 28;
            }
            FeatureClass pointClass = new FeatureClass(points);
            return pointClass;
        }

        FeatureClass ReadPolyLineShp(BinaryReader br)
        {
            List<PolylineFeature> polylines = new List<PolylineFeature>();
            while (byteCount < fileLength)
            {
                PolylineFeature temp = new PolylineFeature(br);
                byteCount += temp.Length;
                polylines.Add(temp);
            }
            FeatureClass polylineClass = new FeatureClass(polylines);
            return polylineClass;
        }

        FeatureClass ReadPolygonShp(BinaryReader br)
        {
            List<PolygonFeature> polygons = new List<PolygonFeature>();
            while (byteCount < fileLength)
            {
                PolygonFeature temp = new PolygonFeature(br);
                byteCount += temp.ByteCount;
                polygons.Add(temp);
            }
            FeatureClass polygonClass = new FeatureClass(polygons);
            return polygonClass;
        }

        public double XMin
        {
            get { return this.xmin; }
        }
        public double YMin
        {
            get { return this.ymin; }
        }
        public double XMax
        {
            get { return this.xmax; }
        }
        public double YMax
        {
            get { return this.ymax; }
        }
    }
}
