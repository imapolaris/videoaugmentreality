using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seecool.VideoAR
{
    public class TargetInfo : ITargetInfo
    {
        public double VideoX { get; set; }
        public double VideoY { get; set; }
        public double SizeX { get; set; }
        public double SizeY { get; set; }
        public object Infomation { get; set; }
    }
}
