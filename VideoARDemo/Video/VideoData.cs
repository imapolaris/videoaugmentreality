using Seecool.VideoAR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VideoStreamClient.Entity;

namespace VideoARDemo
{
    public class VideoData
    {
        private static VideoStreamClient.VideoSourceManager _srcMgr;
        private static VideoStreamClient.VideoSourceManager SrcMgr
        {
            get
            {
                if (_srcMgr == null)
                    _srcMgr = VideoStreamClient.VideoSourceManager.CreateInstance(CCTVInfoManager.Instance.ClientHub);
                return _srcMgr;
            }
        }

        private VideoStreamClient.VideoSource _vSource;
        
        internal VideoStreamClient.VideoSource VideoSource { get { return _vSource; } }

        public VideoData(string videoId, string url)
        {
            _vSource = SrcMgr.GetVideoSource(videoId, url);
        }
    }
}