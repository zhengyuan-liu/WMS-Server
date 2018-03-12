using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace shp读取
{
    struct BBOX
    {
        public double xmin, ymin, xmax, ymax;

        public BBOX(double x1, double y1, double x2, double y2)
        {
            this.xmin = x1;
            this.ymin = y1;
            this.xmax = x2;
            this.ymax = y2;
        }
    }

    class FeatureClass
    {
        ShapeTypes shapetype;
        int count;
        public List<PointFeature> points;
        public List<PolylineFeature> polylines;
        public List<PolygonFeature> polygons;
       
        public FeatureClass()
        {
            shapetype = ShapeTypes.NullShape;
            this.count = 0;
        }

        public FeatureClass(List<PointFeature> pointsList)
        {
            shapetype = ShapeTypes.Point;
            this.points = pointsList;
            this.count = this.points.Count;
        }

        public FeatureClass(List<PolylineFeature> polylinesList)
        {
            shapetype = ShapeTypes.Polyline;
            this.polylines = polylinesList;
            this.count = this.polylines.Count;
        }

        public FeatureClass(List<PolygonFeature> polygonsList)
        {
            shapetype = ShapeTypes.Polygon;
            this.polygons = polygonsList;
            this.count = this.polygons.Count;
        }

        public ShapeTypes ShapeType
        {
            get { return this.shapetype; }
        }

        public int Count
        {
            get { return this.count; }
        }

        public Bitmap DrawBMP(BBOX boundarybox, Bitmap bmp)
        {
            int width = bmp.Width;
            int height = bmp.Height;
            Graphics graphic = Graphics.FromImage(bmp);
            Pen mypen = new Pen(Color.Black);

            Random ran = new Random();
            int r = ran.Next(0, 255);
            int g = ran.Next(0, 255);
            int b = ran.Next(0, 255);
            Color brushcolor = Color.FromArgb(r, g, b);

            switch (shapetype)
            {
                case ShapeTypes.Point:
                    for (int i = 0; i < points.Count; i++)
                    {
                        System.Drawing.Point bmpPoint = points[i].GetBMPPoint(boundarybox, width, height);
                        Brush mybrush = new SolidBrush(Color.Green);
                        Rectangle rect = new Rectangle(bmpPoint.X, bmpPoint.Y, 2, 2);
                        graphic.DrawRectangle(mypen, rect);
                        graphic.FillRectangle(mybrush, rect);
                    }
                    break;
                case ShapeTypes.Polyline:
                    for (int i = 0; i < polylines.Count; i++)
                    {
                        System.Drawing.Point[] bmpPoints = polylines[i].GetBMPPoints(boundarybox, width, height);
                        graphic.DrawLines(mypen, bmpPoints);
                    }
                    break;
                case ShapeTypes.Polygon:
                    for (int i = 0; i < polygons.Count; i++)
                    {
                        System.Drawing.Point[] bmpPoints = polygons[i].GetBMPPoints(boundarybox, width, height);
                        Brush mybrush = new SolidBrush(brushcolor);
                        graphic.DrawPolygon(mypen, bmpPoints);
                        graphic.FillPolygon(mybrush, bmpPoints);
                    }
                    break;
                default:
                    break;
            }
            return bmp;
        }

        public Bitmap DrawBMP(BBOX boundarybox, Bitmap bmp, string style)
        {
            int width = bmp.Width;
            int height = bmp.Height;
            Graphics graphic = Graphics.FromImage(bmp);
            Pen mypen = new Pen(Color.Black);

            Color brushcolor = Color.Gray;
            switch (style)
            {
                case "Red":
                    brushcolor = Color.Red;
                    break;
                case "Green":
                    brushcolor = Color.Green;
                    break;
                case "Blue":
                    brushcolor = Color.Blue;
                    break;
                case "Cyan":
                    brushcolor = Color.Cyan;
                    break;
                case "Magenta":
                    brushcolor = Color.Magenta;
                    break;
                case "Yellow":
                    brushcolor = Color.Yellow;
                    break; 
                default:
                    break;
            }

            switch (shapetype)
            {
                case ShapeTypes.Point:
                    for (int i = 0; i < points.Count; i++)
                    {
                        Point bmpPoint = points[i].GetBMPPoint(boundarybox, width, height);
                        Brush mybrush = new SolidBrush(Color.Green);
                        Rectangle rect = new Rectangle(bmpPoint.X, bmpPoint.Y, 2, 2);
                        graphic.DrawRectangle(mypen, rect);
                        graphic.FillRectangle(mybrush, rect);
                    }
                    break;
                case ShapeTypes.Polyline:
                    for (int i = 0; i < polylines.Count; i++)
                    {
                        Point[] bmpPoints = polylines[i].GetBMPPoints(boundarybox, width, height);
                        graphic.DrawLines(mypen, bmpPoints);
                    }
                    break;
                case ShapeTypes.Polygon:
                    for (int i = 0; i < polygons.Count; i++)
                    {
                        Point[] bmpPoints = polygons[i].GetBMPPoints(boundarybox, width, height);
                        Brush mybrush = new SolidBrush(brushcolor);
                        graphic.DrawPolygon(mypen, bmpPoints);
                        graphic.FillPolygon(mybrush, bmpPoints);
                    }
                    break;
                default:
                    break;
            }
            return bmp;
        }
    }
}
