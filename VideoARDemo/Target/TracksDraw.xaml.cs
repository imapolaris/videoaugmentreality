using Seecool.VideoAR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VideoARDemo.Target;

namespace VideoARDemo
{
    /// <summary>
    /// TracksDraw.xaml 的交互逻辑
    /// </summary>
    public partial class TracksDraw : Canvas
    {
        TracksCanvas _tracks;
        public TracksDraw()
        {
            InitializeComponent();
            _tracks = new TracksCanvas();
            this.Children.Add(_tracks);
            this.SizeChanged += TracksDraw_SizeChanged;

            this.MouseDown += TracksDraw_MouseDown;
        }
        
        private void TracksDraw_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var pt = e.GetPosition(this);
            _tracks.UpdateSelectedTarget(new Point2d((double)pt.X / ActualWidth, (double)pt.Y / ActualHeight));
        }

        private void TracksDraw_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _tracks.UpdateShow(e.NewSize.Width, e.NewSize.Height);
        }

        public void UpdateTargets(ITargetInfo[] infos)
        {
            _tracks.UpdateDynamicTargets(infos);
        }
    }
}
