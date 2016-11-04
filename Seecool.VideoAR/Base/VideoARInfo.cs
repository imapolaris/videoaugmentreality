using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seecool.VideoAR
{
    public class VideoARInfo : IVideoARInfo
    {
        public ITargetInfo[] Targets { get; private set; }

        public string VideoId { get; private set; }

        public VideoARInfo(string videoId, ITargetInfo[] targetInfos)
        {
            VideoId = videoId;
            Targets = targetInfos;
        }
    }
}
