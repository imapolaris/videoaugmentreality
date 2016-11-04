using AopUtil.WpfBinding;
using Seecool.VideoAR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoARDemo
{
    public class VideoARViewModel: ObservableObject, IDisposable
    {
        IVideoAR _ar;
        [AutoNotify]
        public string TextString { get; set; }
        public VideoDisplayViewModel VideoDisplayVM { get; set; }
        public VideoARViewModel(string webApiBaseUri, string dataBusEndpoint, params string[] topics)
        {
            _ar = new VideoARManager(webApiBaseUri, dataBusEndpoint, topics);
            _ar.VideoARInfoEvent += onVideoAR;
            VideoDisplayVM = new VideoDisplayViewModel();
        }

        public void UpdateVideoIds(params string[] videoIds)
        {
            _ar.UpdateVideoIds(videoIds);
            VideoDisplayVM.PlayVideo(videoIds.FirstOrDefault());
        }

        private void onVideoAR(IVideoARInfo obj)
        {
            VideoARInfo info = obj as VideoARInfo;
            //string str = null;
            if (info != null)
            {
                if (TargetsChanged != null)
                    TargetsChanged(info.Targets);
                //str += $"{DateTime.Now}  {info.VideoId} 点数: {info.Targets.Length}\n";
                //Console.WriteLine($"{DateTime.Now}  {info.VideoId} Count: {info.Targets.Length}");
                //foreach (var t in info.Targets)
                //{
                //    var target = t as TargetInfo;
                //    if (target != null)
                //    {
                //        var infom = target.Infomation as Adapter.Proto.ScUnion;
                //        if (infom != null)
                //        {
                //            //str += $"\t[{target.VideoX}, {target.VideoY}] {infom.Name} {infom.ID}  {infom.Longitude} {infom.Latitude} {infom.SOG} {infom.COG} {infom.Length}\n";
                //            Console.WriteLine($"\t[{target.VideoX}, {target.VideoY},{target.SizeX},{target.SizeY}] {infom.Name} {infom.ID}  {infom.Longitude} {infom.Latitude} {infom.SOG} {infom.COG} {infom.Length}");
                //        }
                //    }
                //}
            }
            //TextString = str;
            //Console.WriteLine(TextString);
        }

        public Action<ITargetInfo[]> TargetsChanged;

        public void Dispose()
        {
            _ar.Dispose();
            VideoDisplayVM.Dispose();
        }
    }
}
