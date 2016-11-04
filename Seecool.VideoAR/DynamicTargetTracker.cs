using Adapter.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seecool.VideoAR
{
    public class DynamicTargetTracker
    {
        DateTime _updatedTime;
        public ScUnion Target { get; private set; }
        public string[] VideoIdArray { get; set; }
        public DynamicTargetTracker(ScUnion target)
        {
            UpdateTarget(target);
        }

        public void UpdateTarget(ScUnion target)
        {
            Target = target;
            _updatedTime = DateTime.Now;
        }

        public Position GetPosition(DateTime time)
        {
            if (Target.Longitude < -180 || Target.Longitude > 180 || Target.Latitude >= 90 || Target.Latitude <= -90)
                return null;
            TimeSpan span = time - _updatedTime;
            if (span > ConstSettings.TimeoutSpan)
                return null;
            Position pos = new Position(Target.Longitude, Target.Latitude);
            if(Target.SOG > 1 && Target.COG >= 0 &&Target.COG < 360)
            {
                pos.Lat = Target.Latitude + Target.SOG * Math.Cos(Target.COG * Math.PI / 180) * span.TotalHours / 60;
                pos.Lon = Target.Longitude + Target.SOG * Math.Sin(Target.COG * Math.PI / 180) * span.TotalHours / 60 / Math.Cos(pos.Lat * Math.PI / 180);
            }
            return pos;
        }
    }
}
