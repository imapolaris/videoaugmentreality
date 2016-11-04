namespace Seecool.VideoAR
{
    public class PTZPosition
    {
        /// <summary>经度</summary>
        public double Lon { get; set; }
        /// <summary>纬度</summary>
        public double Lat { get; set; }
        /// <summary>安装高度</summary>
        public double Alt { get; set; }
        /// <summary>当前水平朝向</summary>
        public double Pan { get; set; }
        /// <summary>垂直朝向</summary>
        public double Tilt { get; set;}
        /// <summary>水平视场角</summary>
        public double Viewport { get; set; }
        /// <summary>原始画面像素长宽比</summary>
        public double SizeRatio { get; set; }
        public PTZPosition(double lon, double lat, double alt, double pan, double tilt, double viewport, double sizeRatio)
        {
            Lon = lon;
            Lat = lat;
            Alt = alt;
            Pan = pan;
            Tilt = tilt;
            Viewport = viewport;
            SizeRatio = sizeRatio;
        }

        public bool IsValid()
        {
            return Lon > -180 && Lon <= 180 && Lat > -90 && Lat < 90 
                && Pan >= 0 && Pan < 360 && Tilt > -90 && Tilt < 90 
                && Viewport > 0 && Viewport < 90 && SizeRatio > 0;
        }
    }
}