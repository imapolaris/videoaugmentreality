using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seecool.VideoAR
{
    public class Point2d
    {
        public double X { get; set; }
        public double Y { get; set; }
        public Point2d()
        {
        }

        public Point2d(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    public class Position
    {
        public double Lon { get; set; }
        public double Lat { get; set; }
        public Position()
        {
        }

        public Position(double lon, double lat)
        {
            Lon = lon;
            Lat = lat;
        }
    }
}
