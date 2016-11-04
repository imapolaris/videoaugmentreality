using CCTVClient;
using CCTVInfoHub;
using CCTVInfoHub.Entity;
using CCTVInfoHub.Util;
using CCTVModels;
using Common.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Seecool.VideoAR
{
    /// <summary>
    /// 首次使用请先调用 Init(webApiBaseUri).
    /// </summary>
    public class CCTVInfoManager
    {
        public static CCTVInfoManager Instance { get; private set; } = new CCTVInfoManager();
        string _webApiBaseUri;
        ManualResetEvent _disposeEvent = new ManualResetEvent(false);
        /// <summary>
        /// 初始化WebApiUri
        /// </summary>
        /// <param name="webApiBaseUri"></param>
        public void Init(string webApiBaseUri)
        {
            _webApiBaseUri = webApiBaseUri;
            init();
            _disposeEvent.Reset();
            new Thread(runFeedbackPTZ) { IsBackground = true }.Start();
        }

        private void runFeedbackPTZ()
        {
            int count = 0;
            while (!_disposeEvent.WaitOne(400))
            {
                lock (_dictPTZDynamics)
                {
                    if (count % 25 == 0)    //静态信息每秒刷新一次
                    {
                        foreach (var dict in _dictPTZStatics)
                        {
                            if (dict.Value != null)
                            {
                                var info = GetStaticInfo(dict.Key);
                                PTZPosition ptz = info != null ? new PTZPosition(info.Longitude, info.Latitude, info.Altitude, info.Heading, 90 - info.Tilt, info.ViewPort, info.SizeRatio) : null;
                                dict.Value(ptz);
                            }
                        }
                        count = 0;
                    }
                    foreach (var dict in _dictPTZDynamics)  //动态信息每帧刷新一次
                    {
                        if (dict.Value != null)
                        {
                            var info = GetDynamicInfo(dict.Key);
                            //Console.WriteLine($"{DateTime.Now.TimeOfDay} DynamicInfo: Lon: {info.Longitude},Lat: {info.Latitude},\t Alt: {info.Altitude},\tPT[{info.Heading},{info.Tilt}], Viewport: {info.ViewPort}");
                            if(info.Longitude > 180 || info.Longitude < -180 || info.Latitude > 90 || info.Latitude < -90)
                            {
                                var staticInfo = GetStaticInfo(dict.Key);
                                info.Longitude = staticInfo.Longitude;
                                info.Latitude = staticInfo.Latitude;
                                info.Altitude = staticInfo.Altitude;
                                info.SOG = 0;
                                info.COG = 0;
                            }
                            PTZPosition ptz = info != null? new PTZPosition(info.Longitude, info.Latitude, info.Altitude, info.Heading, 90 - info.Tilt, info.ViewPort, 0): null;
                            if (ptz != null)
                                ptz = updateDictPTZ(dict.Key, ptz);
                            dict.Value(ptz);
                        }
                    }
                }
                count++;
            }
        }

        const string _defaultHierarchy = "Default";
        private string _currentTree = "Default";
        private CCTVHierarchyNode _rootNode;
        bool _fromConfig = false;
        public CCTVHierarchyNode GetHierarchy(string hierarchyName = _defaultHierarchy)
        {
            if (!hierarchyName.Equals(_currentTree, StringComparison.OrdinalIgnoreCase))
            {
                _rootNode = null;
                _currentTree = hierarchyName;
                ClientHub.RegisterHierarchy(hierarchyName, TimeSpan.FromSeconds(5), hierUpdated);
            }
            //目前只能获取默认节点树。
            if (_rootNode == null)
            {
                CCTVHierarchyNode[] roots = ClientHub.GetAllHierarchyRoots();
                if (roots != null && roots.Length > 0)
                {
                    if (roots.Length == 1)
                    {
                        _rootNode = roots[0];
                    }
                    else
                    {
                        string id = Guid.NewGuid().ToString();
                        _rootNode = new CCTVHierarchyNode()
                        {
                            Name = "根节点",
                            Id = id,
                            Type = NodeType.Server,
                            ElementId = id,
                            Children = roots
                        };
                    }
                }
            }
            return _rootNode;
            //if (rootNode == null)
            //    ClientHub.UpdateRegistered<HierarchyInfo>();
            //return rootNode;
        }

        private void hierUpdated(IEnumerable<string> keys)
        {
            _rootNode = null;
        }


        public string GetVideoReadableName(string videoId, string hierarchyName = _defaultHierarchy)
        {
            //目前只能获取默认节点树里面的名称。
            return GetVideoReadableName(videoId);
        }

        private string GetVideoReadableName(string videoId)
        {
            CCTVHierarchyNode hierarchy = GetHierarchy(_currentTree);
            if (hierarchy != null)
            {
                string name = getVideoReadableName(hierarchy, videoId);
                if (name != null)
                    return name;
            }
            return null;
        }

        private string getVideoReadableName(CCTVHierarchyNode node, string videoId)
        {
            CCTVHierarchyNode video = node.Children.FirstOrDefault(x => x.ElementId == videoId);
            if (video == null)
            {
                foreach (CCTVHierarchyNode child in node.Children)
                {
                    string name = getVideoReadableName(child, videoId);
                    if (name != null)
                        return name;
                }
            }
            else
                return $"{node.Name} - {video.Name}";

            return null;
        }

        public CCTVStaticInfo GetStaticInfo(string videoId)
        {
            ClientHub.UpdateDefault(CCTVInfoType.StaticInfo);
            return ClientHub.GetStaticInfo(videoId);
        }

        public CCTVControlConfig GetControlConfig(string videoId)
        {
            return ClientHub.GetControlConfig(videoId);
        }

        public CCTVDynamicInfo GetDynamicInfo(string videoId)
        {
            ClientHub.UpdateDefault(CCTVInfoType.DynamicInfo);
            return ClientHub.GetDynamicInfo(videoId);
        }

        Dictionary<string, Action<PTZPosition>> _dictPTZDynamics = new Dictionary<string, Action<PTZPosition>>();
        Dictionary<string, Action<PTZPosition>> _dictPTZStatics = new Dictionary<string, Action<PTZPosition>>();
        Dictionary<string, PTZPosition> _dictPTZ = new Dictionary<string, PTZPosition>();
        public void AddPTZReceived(string videoId, Action<PTZPosition> ptzEvent)
        {
            lock(_dictPTZDynamics)
            {
                if (_dictPTZDynamics.ContainsKey(videoId))
                    _dictPTZDynamics[videoId] = ptzEvent;
                else if (_dictPTZStatics.ContainsKey(videoId))
                    _dictPTZStatics[videoId] = ptzEvent;
                else
                {
                    if (GetControlConfig(videoId) != null)
                        _dictPTZDynamics.Add(videoId, ptzEvent);
                    else
                    {
                        CCTVStaticInfo info = GetStaticInfo(videoId);
                        PTZPosition ptz = info != null ? new PTZPosition(info.Longitude, info.Latitude, info.Altitude, info.Heading, info.Tilt, info.ViewPort, info.SizeRatio) : null;
                        _dictPTZStatics.Add(videoId, ptzEvent);
                        ptzEvent(ptz);
                    }
                }
            }
        }

        public void RemovePTZReceived(string videoId)
        {
            lock (_dictPTZDynamics)
            {
                _dictPTZDynamics.Remove(videoId);
            }
        }
        
        public CCTVDefaultInfoSync ClientHub { get; private set; }
        private void init()
        {
            if (string.IsNullOrEmpty(_webApiBaseUri))
                return;
            Console.WriteLine(_webApiBaseUri);
            ClientHub = new CCTVDefaultInfoSync(_webApiBaseUri);
            ClientHub.RegisterDefaultWithoutUpdate(CCTVInfoType.GlobalInfo);
            ClientHub.RegisterDefault(CCTVInfoType.HierarchyInfo, TimeSpan.FromSeconds(10));
            ClientHub.RegisterDefault(CCTVInfoType.ControlConfig, TimeSpan.FromSeconds(10));
            ClientHub.RegisterDefault(CCTVInfoType.StaticInfo, TimeSpan.FromSeconds(10));
            ClientHub.RegisterDefault(CCTVInfoType.DynamicInfo, TimeSpan.FromSeconds(5));
        }

        PTZPosition updateDictPTZ(string videoId, PTZPosition pos)
        {
            if (_dictPTZ.ContainsKey(videoId))
            {
                if (pos.SizeRatio <= 0 && _dictPTZ[videoId].SizeRatio > 0)
                    pos.SizeRatio = _dictPTZ[videoId].SizeRatio;
                _dictPTZ[videoId] = pos;
            }
            else
                _dictPTZ.Add(videoId, pos);
            return _dictPTZ[videoId];
        }
    }
}
