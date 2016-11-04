using Seecool.VideoAR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace VideoARDemo.Target
{
    public class TracksCanvas : Canvas, IDisposable
    {
        private ConcurrentDictionary<string, TrackCanvas> _dynamicObjectEvent = new ConcurrentDictionary<string, TrackCanvas>();
        public void UpdateDynamicTargets(ITargetInfo[] infos)
        {
            this.Dispatcher.BeginInvoke((Action)delegate ()
            {
                lock (_dynamicObjectEvent)
                {
                    if (infos == null || infos.Length == 0)
                    {
                        clear();
                    }
                    var ids = infos.Select(_ => (_.Infomation as Adapter.Proto.ScUnion)?.ID).ToArray();
                    foreach (var id in _dynamicObjectEvent.Keys)
                    {
                        if (!ids.Any(_ => _ == id))
                            remove(id);
                    }
                    for (int i = 0; i < ids.Length; i++)
                    {
                        var id = ids[i];
                        if (_dynamicObjectEvent.ContainsKey(id))
                        {
                            _dynamicObjectEvent[id].Update(infos[i]);
                        }
                        else
                        {
                            TrackCanvas canvas = new TrackCanvas(infos[i], Width, Height);
                            _dynamicObjectEvent.TryAdd(id, canvas);
                            this.Children.Add(canvas);
                        }
                    }
                }
            });
        }

        public void UpdateShow(double width, double height)
        {
            Width = width;
            Height = height;
            lock(_dynamicObjectEvent)
            {
                foreach (var key in _dynamicObjectEvent.Keys)
                {
                    TrackCanvas track;
                    if (_dynamicObjectEvent.TryGetValue(key, out track))
                        track.UpdateShow(Width, Height);
                }
            }
        }

        private void remove(string id)
        {
            TrackCanvas canvas;
            if(_dynamicObjectEvent.TryRemove(id, out canvas))
                this.Children.Remove(canvas);
        }

        private void clear()
        {
            _dynamicObjectEvent.Clear();
            this.Children.Clear();
        }

        public void UpdateSelectedTarget(Point2d point)
        {
            foreach (var key in _dynamicObjectEvent.Keys.ToArray())
            {
                TrackCanvas target;
                if (_dynamicObjectEvent.TryGetValue(key, out target))
                {
                    if (isNeerPoint(key, point))
                    {

                        //if (target.Information.Infomation is Adapter.Proto.ScUnion)
                        //    System.Diagnostics.Debug.WriteLine(((Adapter.Proto.ScUnion)target.Information.Infomation).V_Name);
                        //else
                        //    System.Diagnostics.Debug.WriteLine("不知道是啥！");

                        target.Icon.Rect.Selected = true;

                        System.Diagnostics.Debug.WriteLine((target.Information.Infomation as Adapter.Proto.ScUnion).Name);
                    }
                    else
                    {
                        target.Icon.Rect.Selected = false;
                    }
                }
            }
        }

        private bool isNeerPoint(string id, Point2d point)
        {
            TrackCanvas target;
            if (_dynamicObjectEvent.TryGetValue(id, out target))
            {
                if (target.IsPointInside(point))
                    return true;
            }

            return false;
        }

        public void Dispose()
        {
        }
    }
}
