using Seecool.VideoAR.DataBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Adapter.Proto;
using System.Threading;

namespace Seecool.VideoAR
{
    /// <summary>
    /// 视频增强模块
    /// </summary>
    public class VideoARManager : IVideoAR, IDisposable
    {
        DataBusDataReceiver _dataBus;
        Dictionary<string, VideoTargetTracker> _dictVTTracker = new Dictionary<string, VideoTargetTracker>();
        Dictionary<string, DynamicTargetTracker> _dynamics = new Dictionary<string, DynamicTargetTracker>();
        ManualResetEvent _disposeEvent = new ManualResetEvent(false);
        public Action<IVideoARInfo> VideoARInfoEvent { get; set; }
        /// <summary>视频增强模块</summary>
        /// <param name="webApiBaseUri">as "http://192.168.9.222:27010/"</param>
        /// <param name="dataBusEndpoint">as "tcp://192.168.9.222:62626"</param>
        /// <param name="topics">as { "ScUnion" }</param>
        public VideoARManager(string webApiBaseUri, string dataBusEndpoint, params string[] topics)
        {
            SetCCTVConfig(webApiBaseUri);
            SetDataBaseConfig(dataBusEndpoint, topics);
            _disposeEvent.Reset();
            new Thread(run) { IsBackground = true }.Start();
        }

        private void run()
        {
            while (!_disposeEvent.WaitOne(1000))
            {
                var targetArray = _dynamics.Values.ToArray();
                var cctvArray = _dictVTTracker.Values.ToArray();
                foreach(var cctv in cctvArray)
                {
                    var targets = targetArray.Where(target => target.VideoIdArray.Any(id => id == cctv.VideoId));
                    List<TargetInfo> list = new List<TargetInfo>();
                    foreach(var t in targets)
                    {
                        var targetInfo = getTargetInfo(cctv, t);
                        if (targetInfo != null)
                            list.Add(targetInfo);
                    }
                    onVideoARInfo(new VideoARInfo(cctv.VideoId, list.ToArray()));
                }
            }
        }

        TargetInfo getTargetInfo(VideoTargetTracker cctv, DynamicTargetTracker target)
        {
            var pos = target.GetPosition(DateTime.Now);
            if (pos != null)
            {
                var t = target.Target;
                double length = t.Length > 0 ? t.Length: 20;
                double measureA = length / 2;
                double measureC = length / 2;
                if (t.MeasureA > 0 && t.MeasureC > 0)//
                {
                    Console.WriteLine("Measure: " + t.MeasureA + " " + t.MeasureC);
                    measureA = t.MeasureA;
                    measureC = t.MeasureC;
                }
                double width = t.Width > 0 ? t.Width : length / 6;
                                
                Position[] posits = getRectVertexLonLat(t.Longitude, t.Latitude, t.SOG, measureA, measureC, width / 2, width / 2);
                var pts = posits.Select(p => cctv.GetVideoPosition(p.Lon, p.Lat, 0, t.Length)).ToArray();
                if(pts.All(pt => pt != null))
                {
                    double left = Math.Max(0, pts.Min(pt => pt.X));
                    double right = Math.Min(1, pts.Max(pt => pt.X));
                    double up = Math.Max(0, pts.Min(pt => pt.Y));
                    double down = Math.Min(1, pts.Max(pt => pt.Y));
                    if (left < right && up < down)
                    {
                        //船高修正
                        int index = 0;
                        for (; index < pts.Length; index++)
                        {
                            if (up == pts[index].Y)
                                break;
                        }
                        Point2d upCorrect = cctv.GetVideoPosition(posits[index].Lon, posits[index].Lat, 3, t.Length);//船高3米修正
                        up = Math.Max(0, upCorrect?.Y ?? 1);

                        return new TargetInfo()
                        {
                            VideoX = (left + right) / 2,
                            VideoY = (up + down) / 2,
                            SizeX = right - left,
                            SizeY = down - up,
                            Infomation = t,
                        };
                    }
                        
                }

                //var pt = cctv.GetVideoPosition(pos.Lon, pos.Lat, 0, target.Target.Length);
                //if (pt != null && pt.X > -0.1 && pt.X < 1.1 && pt.Y > 0 && pt.Y < 1.1)
                //    return new TargetInfo() { VideoX = pt.X, VideoY = pt.Y, SizeX = 0.05, SizeY = 0.04, Infomation = target.Target };
            }
            return null;
        }

        /// <summary>获取目标四个顶点坐标 <summary>
        public Position[] getRectVertexLonLat(double targetLon, double targetLat, double heading, double measureA, double measureC, double measureB, double measureD)
        {
            List<Position> posits = new List<Position>();
            double cos = Math.Cos(heading * Math.PI / 180);
            double sin = Math.Cos(heading * Math.PI / 180);

            double ltX = sin * measureA - cos * measureB;
            double ltY = cos * measureA + sin * measureB;
            double rtX = sin * measureA + cos * measureB;
            double rtY = cos * measureA + sin * measureB;

            double ldX = -sin * measureC - cos * measureB;
            double ldY = -cos * measureC + sin * measureB;
            double rdX = -sin * measureC + cos * measureB;
            double rdY = -cos * measureC + sin * measureB;

            double cosLat = Math.Cos(targetLat * Math.PI / 180);
            posits.Add(new Position(targetLon + ltX /1852.0 / 60 / cosLat,targetLat + ltY / 1852.0 / 60));
            posits.Add(new Position(targetLon + rtX /1852.0 / 60 / cosLat,targetLat + rtY / 1852.0 / 60));
            posits.Add(new Position(targetLon + ldX /1852.0 / 60 / cosLat,targetLat + ldY / 1852.0 / 60));
            posits.Add(new Position(targetLon + rdX / 1852.0 / 60 / cosLat,targetLat + rdY / 1852.0 / 60));
            return posits.ToArray();
        }
        
        private void onVideoARInfo(VideoARInfo videoARInfo)
        {
            var handler = VideoARInfoEvent;
            if(handler != null)
                handler(videoARInfo);
        }

        public void SetCCTVConfig(string webApiBaseUri)
        {
            CCTVInfoManager.Instance.Init(webApiBaseUri);
        }

        /// <summary>
        /// databus参数配置
        /// </summary>
        /// <param name="endpoint">as "tcp://127.0.0.1:62626"</param>
        /// <param name="topics">as { "ScUnion" }</param>
        public void SetDataBaseConfig(string endpoint, string[] topics)
        {
            _dataBus?.Dispose();
            _dataBus = new DataBusDataReceiver(endpoint, topics);
            _dataBus.DynamicEvent += onDynamic;
        }

        private void onDynamic(ScUnion obj)
        {
            var array = _dictVTTracker.Values.Where(_ => _.IsInOrNeerCCTV(obj.Longitude, obj.Latitude)).Select(_ => _.VideoId).ToArray();
            //TODO 更新所在视频区域列表，更新位置信息
            if (array.Length == 0)
            {
                if (_dynamics.ContainsKey(obj.ID))
                    _dynamics.Remove(obj.ID);
            }
            else
            {
                if (_dynamics.ContainsKey(obj.ID))
                    _dynamics[obj.ID].UpdateTarget(obj);
                else
                    _dynamics.Add(obj.ID, new DynamicTargetTracker(obj));
                _dynamics[obj.ID].VideoIdArray = array;
            }
        }

        public void UpdateVideoIds(params string[] videoIds)
        {
            if (videoIds == null || videoIds.Length == 0)
                _dictVTTracker.Clear();
            else
            {
                List<string> keys = _dictVTTracker.Keys.ToList();
                //增加新增的视频Id
                foreach (var id in videoIds)
                {
                    if(!_dictVTTracker.ContainsKey(id))
                    {
                        _dictVTTracker.Add(id, new VideoTargetTracker(id));
                        keys.Remove(id);
                    }
                }
                //删除旧的的视频Id
                foreach (var key in keys)//
                    _dictVTTracker.Remove(key);
            }
        }

        public void Dispose()
        {
            _disposeEvent.Set();
            UpdateVideoIds();
            if(_dataBus != null)
            {
                _dataBus.Dispose();
                _dataBus = null;
            }
        }
    }
}
