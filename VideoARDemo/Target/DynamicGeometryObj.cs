using Seecool.VideoAR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace VideoARDemo.Target
{
    public class DynamicGeometryObj : Canvas, IDisposable
    {
        double _width = 0;
        double _height = 0;
        ITargetInfo _target;
        RectangleObj _rect;
        TextBlock _title;
        public DynamicGeometryObj(ITargetInfo target, double width, double height)
        {
            _target = target;
            _rect = new RectangleObj(10, 10, Brushes.Red, new SolidColorBrush(Color.FromArgb(60, 0, 255, 0)));
            this.Children.Add(_rect);
            UpdateShow(width, height);

            Rect = _rect;

            // title
            _title = new TextBlock();
            _title.Text = ((Adapter.Proto.ScUnion)_target.Infomation).Name;

            this.Children.Add(_title);
        }
        
        public void UpdateTarget(ITargetInfo target)
        {
            _target = target;
            updateShow();
        }

        public void UpdateShow(double width, double height)
        {
            _width = width;
            _height = height;
            updateShow();
        }

        private void updateShow()
        {
            //Console.WriteLine(_target.SizeX * _width + " ，" + _target.SizeY * _height);
            _rect.UpdateSize(_target.SizeX * _width, _target.SizeY * _height);
        }

        public void Dispose()
        {
            this.Children.Clear();
        }

        public RectangleObj Rect { get; private set; }
    }
}
