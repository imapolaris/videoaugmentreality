using Seecool.VideoAR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using Adapter.Proto;
using System.Windows;

namespace VideoARDemo.Target
{
    public class TrackCanvas : Canvas, IDisposable
    {
        DynamicGeometryObj _icon;
        ITargetInfo _info;
        public TrackCanvas(ITargetInfo info, double width, double height)
        {
            _info = info;
            _icon = new DynamicGeometryObj(info,width, height);
            this.Children.Add(_icon);
            UpdateShow(width, height);

            PointInScreen = new Point(info.VideoX, info.VideoY);
            TrackSize = new Size(info.SizeX, info.SizeY);
            Icon = _icon;
        }

        public void Dispose()
        {
            this.Children.Clear();
            _icon.Dispose();
        }

        internal void Update(ITargetInfo info)
        {
            _info = info;
            _icon.UpdateTarget(info);
            updateShow();
        }
        double _width = 0;
        double _height = 0;
        public void UpdateShow(double width, double height)
        {
            _width = width;
            _height = height;
            _icon.UpdateShow(width, height);
            updateShow();
        }

        private void updateShow()
        {
            Canvas.SetLeft(_icon, _info.VideoX * _width);
            Canvas.SetTop(_icon, _info.VideoY * _height);
            //double x = Math.Min(Math.Max(0, _info.VideoX),1);
            //double y = Math.Min(Math.Max(0, _info.VideoY),1);
            //Canvas.SetLeft(_icon, x * _width);
            //Canvas.SetTop(_icon, y * _height);
        }

        public Point PointInScreen { get; private set; }
        public Size TrackSize { get; private set; }
        public DynamicGeometryObj Icon { get; private set; }

        public bool IsPointInside(Point2d point)
        {
            //LogHelperEx.WriteLog($"x:{PointInScreen.X},y:{PointInScreen.Y}\n"+$"width:{TrackSize.Width},height:{TrackSize.Height}\n"+$"point:x:{point.X},y:{point.Y}");

            if ((point.X >= PointInScreen.X - TrackSize.Width / 2.0 && point.X <= PointInScreen.X + TrackSize.Width / 2.0)
                && (point.Y >= PointInScreen.Y - TrackSize.Height / 2.0 && point.Y <= PointInScreen.Y + TrackSize.Height / 2.0))
                return true;
            else
                return false;
        }

        public ITargetInfo Information { get { return this._info; } }
    }
}
