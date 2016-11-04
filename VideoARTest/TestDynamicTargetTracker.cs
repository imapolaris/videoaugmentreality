using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Seecool.VideoAR;
using Adapter.Proto;

namespace VideoARTest
{
    [TestClass]
    public class TestDynamicTargetTracker
    {
        [TestMethod]
        public void TestDynamicTargetTracker_GetPosition()
        {
            //向东行驶
            ScUnion ship = new ScUnion() { Longitude = 121, Latitude = 60, SOG = 9.0f, COG = 90, Width = 60 };
            DynamicTargetTracker tarcker = new DynamicTargetTracker(ship);
            Position pos = tarcker.GetPosition(DateTime.Now.AddSeconds(10));
            Assert.AreEqual(Math.Round(121 + 9.0 * 10 / 3600 / 60 / Math.Cos(Math.PI * ship.Latitude / 180), 7), Math.Round(pos.Lon, 7));// / Cos(60)=0.5
            Assert.AreEqual(60, pos.Lat);
            //向北行驶
            ship.COG = 0;
            Position pos1 = tarcker.GetPosition(DateTime.Now.AddSeconds(10));
            Assert.AreEqual(121, pos1.Lon);
            Assert.AreEqual(Math.Round(60 + 9.0 * 10 / 3600 / 60, 7), Math.Round(pos1.Lat, 7));
        }
        
        [TestMethod]
        public void TestDynamicTargetTracker_Track()
        {

        }
    }
}
