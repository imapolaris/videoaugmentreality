using Adapter.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seecool.VideoAR
{
    public class CameraCalculator
    {
        public PTZPosition PTZ { get; private set; }
        bool _isValidCameraData = false;
        /// <summary>
        /// 摄像机相关目标位置计算
        /// </summary>
        public CameraCalculator(PTZPosition pos)
        {
            UpdatePTZ(pos);
        }

        public void UpdatePTZ(PTZPosition pos)
        {
            if (pos.SizeRatio == 0)
                pos.SizeRatio = 16.0 / 9;
            bool posChanged = PTZ == null || !PTZ.IsValid() || PTZ.Lon != pos.Lon || PTZ.Lat != pos.Lat || PTZ.Alt != pos.Alt;
            PTZ = pos;
            _isValidCameraData = pos.IsValid();
            if (posChanged && _isValidCameraData)
                initLimit();
        }

        public Point2d GetPosInVideo(double lon, double lat, double alt, double length = 0)
        {
            double distance = Calculator.CalcDis(PTZ.Lon, PTZ.Lat, lon, lat);
            if (distance > ConstSettings.DistanceSup)
                return null;
            double pan = Calculator.CalcDirection(PTZ.Lon, PTZ.Lat, lon, lat);
            double tilt = Math.Atan((PTZ.Alt - alt) / 1852 / distance) * 180 / Math.PI;
            Point2d pt = GetScreenPosFromPT(PTZ.Pan, PTZ.Tilt, PTZ.Viewport, PTZ.SizeRatio, pan, tilt);
            //Console.WriteLine($"PT[{pan},{tilt}] Point[{pt.X}, {pt.Y}] Camera[{_pos.Pan}, {_pos.Tilt},{_pos.Viewport},{_pos.SizeRatio}]");
            return pt;
        }

        public bool IsInOrNearMonitor(double lon, double lat)
        {
            return _isValidCameraData && inCamaraArea(lon, lat);
        }

        /// <summary>已知云台PTZ,目标PT，获取目标在图像中的坐标</summary>
        /// <param name="ptzZoom">云台Zoom值</param>
        /// <param name="camTilt">云台Tilt值</param>
        /// <param name="camPan">云台Pan值</param>
        /// <param name="ptTilt">目标Tilt值</param>
        /// <param name="ptPan">目标Pan值</param>
        public static Point2d GetScreenPosFromPT(double camPan, double camTilt, double viewport, double sizeRatio, double ptPan, double ptTilt)
        {
            double jiajiao = GetAngle(camPan, camTilt, ptPan, ptTilt);
            if (jiajiao >= 90)
                return null;
            double focalLength = getFocalLength(viewport);
            double ApOppBp = (ptPan - camPan) * Math.PI / 180;
            double OpBpOpp = Math.Atan(Math.Tan(ptTilt * Math.PI / 180) / Math.Cos(ApOppBp));
            double OOpBp = OpBpOpp - camTilt * Math.PI / 180;
            double deltaY = focalLength * Math.Tan(OOpBp);
            double ptY = 0.5f + deltaY * sizeRatio;
            double tanAOpB = Math.Tan(ApOppBp) * Math.Cos(OpBpOpp);
            double deltaX = tanAOpB * Math.Sqrt(deltaY * deltaY + focalLength * focalLength);
            double ptX = 0.5 + deltaX;
            return new Point2d(ptX, ptY);
        }
        
        private static double getFocalLength(double angle)
        {
            double focalLength = 0.5 / Math.Tan(angle * Math.PI / 180 / 2);
            return focalLength;
        }

        #region 默认摄像机监控范围
        double _lonInf;
        double _lonSup;
        double _latInf;
        double _latSup;

        void initLimit()
        {
            double cosLat = Math.Cos(PTZ.Lat * Math.PI / 180);
            double disSup = ConstSettings.DistanceSup / 60;
            _lonInf = PTZ.Lon - disSup / cosLat;
            _lonSup = PTZ.Lon + disSup / cosLat;
            _latInf = PTZ.Lat - disSup;
            _latSup = PTZ.Lat + disSup;
        }

        bool inCamaraArea(double lon, double lat)
        {
            return lon <= _lonSup && lon >= _lonInf && lat <= _latSup && lat >= _latInf;
        }
        #endregion 默认摄像机监控范围

        /// <summary>计算相机对应球坐标系中夹角</summary>
        /// <param name="pan1">目标1 Pan值</param>
        /// <param name="tilt1">目标1 Tilt值</param>
        /// <param name="pan2">目标2 Pan值</param>
        /// <param name="tilt2">目标2 Tilt值</param>
        /// <returns>夹角值（度）</returns>
        public static double GetAngle(double pan1, double tilt1, double pan2, double tilt2)
        {
            double cos = Math.Cos(tilt1 * Math.PI / 180) * Math.Cos(tilt2 * Math.PI / 180) * Math.Cos((pan1 - pan2) * Math.PI / 180);
            double sin = Math.Sin(tilt1 * Math.PI / 180) * Math.Sin(tilt2 * Math.PI / 180);
            return Math.Acos(cos+ sin) * 180 / Math.PI;
        }
    }
}