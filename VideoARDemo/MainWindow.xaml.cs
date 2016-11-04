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
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new VideoARViewModel("http://192.168.9.222:27010/", "tcp://192.168.9.222:62626", "Transas_ScUnion");
            ViewModel.UpdateVideoIds("CCTV1_50BAD15900030303");
            ViewModel.TargetsChanged += onTragetsChanged;
            this.Closed += MainWindow_Closed;
        }

        private void onTragetsChanged(ITargetInfo[] infos)
        {
            tracks.UpdateTargets(infos);
        }

        public VideoARViewModel ViewModel { get { return this.DataContext as VideoARViewModel; } }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            ViewModel?.Dispose();
        }
    }
}
