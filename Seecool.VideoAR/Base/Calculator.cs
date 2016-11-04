using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seecool.VideoAR
{
    public static class Calculator
    {
        public static double CalcDis(double lon1, double lat1, double lon2, double lat2)
        {
            double x1 = Math.Cos(lat1 * Math.PI / 180) * Math.Sin(lon1 * Math.PI / 180);
            double y1 = Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lon1 * Math.PI / 180);
            double z1 = Math.Sin(lat1 * Math.PI / 180);
            double x2 = Math.Cos(lat2 * Math.PI / 180) * Math.Sin(lon2 * Math.PI / 180);
            double y2 = Math.Cos(lat2 * Math.PI / 180) * Math.Cos(lon2 * Math.PI / 180);
            double z2 = Math.Sin(lat2 * Math.PI / 180);
            double d = x1 * x2 + y1 * y2 + z1 * z2;
            if (d > 1)
                d = 1;
            return Math.Acos(d) * 180 / Math.PI * 60;
        }

        public static double CalcDirection(double lon1, double lat1, double lon2, double lat2)
        {
            double deltaX = (lon2 - lon1) * Math.Cos(lat1 * Math.PI / 180);
            double deltaY = lat2 - lat1;
            double angle = (90 - Math.Atan2(deltaY, deltaX) * 180 / Math.PI);
            return GetStandardAngle(angle);
        }

        public static double GetAngle(double cog1, double cog2)
        {
            int delta = (int)Math.Abs(cog1 - cog2);
            return Math.Min(delta, 360 - delta);
        }

        /// <summary>
        /// 将角度值转化为0-360度之间
        /// </summary>
        /// <param video="angle">要转化的角度值</param>
        /// <returns></returns>
        public static double GetStandardAngle(double angle)
        {
            while (angle < 0)
                angle += 360;
            while (angle >= 360)
                angle -= 360;
            return angle;
        }
    }
}
