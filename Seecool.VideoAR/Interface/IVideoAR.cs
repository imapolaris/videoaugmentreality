using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seecool.VideoAR
{
    public interface IVideoAR:IDisposable
    {
        void SetDataBaseConfig(string endpoint, string[] topics);
        void SetCCTVConfig(string webApiBaseUri);
        void UpdateVideoIds(params string[] videoIds);
        Action<IVideoARInfo> VideoARInfoEvent { get; set; }
    }
}
