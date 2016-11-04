using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seecool.VideoAR
{
    public interface ITargetInfo
    {
        double VideoX { get; }
        double VideoY { get; }
        double SizeX { get; }
        double SizeY { get; }
        object Infomation { get; }
    }
}
