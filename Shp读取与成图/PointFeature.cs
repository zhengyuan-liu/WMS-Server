using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace shp读取
{
    struct PointD
    {
        double x, y;

        public PointD(double x0,double y0)
        {
            this.x = x0;
            this.y = y0;
        }

        public double X
        {
            get { return this.x; }
        }

        public double Y
        {
            get { return this.y; }
        }
    }

    class PointFeature
    {
        static ShapeTypes shapeType = ShapeTypes.Point;
        static int shapeTypeID = 1;
        double x;
        double y;

        public PointFeature(double x0,double y0)
        {
            this.x = x0;
            this.y = y0;
        }

        public Point GetBMPPoint(BBOX boundarybox, int width, int height)
        {
            double x = width * (this.x - boundarybox.xmin) / (boundarybox.xmax - boundarybox.xmin);
            double y = height * (this.y - boundarybox.ymin) / (boundarybox.ymax - boundarybox.ymin);
            Point bmpPoint = new Point((int)x, height - (int)y);
            return bmpPoint;
        }
    }
}
