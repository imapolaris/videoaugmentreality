using Adapter.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seecool.VideoAR
{
    public class VideoTargetTracker
    {
        public string VideoId { get; private set; }
        public Action<PTZPosition> _ptzEvent;
        object _objLock = new object();
        CameraCalculator _camCalc;
        public PTZPosition PTZ { get { return _camCalc?.PTZ; } }
        public VideoTargetTracker(string videoId)
        {
            VideoId = videoId;
            _ptzEvent += onPTZ;
            CCTVInfoManager.Instance.AddPTZReceived(VideoId, _ptzEvent);
            var staticInfo = CCTVInfoManager.Instance.GetStaticInfo(videoId);
            if(staticInfo != null)
                _camCalc = new CameraCalculator(new PTZPosition(staticInfo.Longitude, staticInfo.Latitude, staticInfo.Altitude, staticInfo.Heading, staticInfo.Tilt, staticInfo.ViewPort, staticInfo.SizeRatio));
        }

        private void onPTZ(PTZPosition ptz)
        {
            lock(_objLock)
            {
                _camCalc?.UpdatePTZ(ptz);
            }
            //if (ptz != null)
            //    Console.WriteLine($"{DateTime.Now.TimeOfDay}\t VideoId: {VideoId}, Lon: {ptz.Lon} Lat: {ptz.Lat} Alt: {ptz.Alt} Pan: {ptz.Pan}, Tilt:　{ptz.Tilt}, Zoom: {ptz.Viewport}");
            //else
            //    Console.WriteLine($"{DateTime.Now.TimeOfDay}\t VideoId: {VideoId}, No PTZ Valid Feedback!");
        }

        public bool IsInOrNeerCCTV(double lon, double lat)
        {
            return _camCalc?.IsInOrNearMonitor(lon, lat) ?? false;
        }

        public Point2d GetVideoPosition(double lon, double lat, double alt, double length)
        {
            return _camCalc?.GetPosInVideo(lon, lat, alt, length);
        }
    }
}
