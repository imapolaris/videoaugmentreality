using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seecool.VideoAR
{
    public static class ConstSettings
    {
        public static double DistanceSup = 1;//最大监控距离：海里
        public static TimeSpan TimeoutSpan = TimeSpan.FromSeconds(60);//目标预测超时时间
    }
}
