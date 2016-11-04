using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Seecool.VideoAR;

namespace VideoARTest.CCTV
{
    [TestClass]
    public class TestCameraCalculator
    {
        double sizeRatio = 16.0 / 9;
        double viewportHor = 60;
        double viewportVer;
        double camPan = 70;
        double camTilt = 10;//>0
        [TestInitialize]
        public void Setup()
        {
            viewportVer = Math.Atan(Math.Tan(viewportHor / 2 * Math.PI / 180) / sizeRatio) * 180 / Math.PI * 2;
        }
        [TestMethod]
        public void TestCameraCalculator_GetAngle()
        {
            Assert.AreEqual(50, Math.Round(CameraCalculator.GetAngle(100, 0, 150, 0),3));
            Assert.IsTrue(30 -Math.Round(CameraCalculator.GetAngle(100, 60, 160, 60),3) > 0);
        }

        [TestMethod]
        public void TestGetScreenPosFromPTMid()//中心点
        {
            Point2d pt = CameraCalculator.GetScreenPosFromPT(camPan, camTilt, viewportHor, sizeRatio, camPan, camTilt);
            Assert.AreEqual(0.5, pt.X);
            Assert.AreEqual(0.5, pt.Y);
        }

        [TestMethod]
        public void TestGetScreenPosFromPTRight()//两侧点(非正点)
        {
            Point2d ptR = CameraCalculator.GetScreenPosFromPT(camPan, camTilt, viewportHor, sizeRatio, camPan + viewportHor / 2, camTilt);
            Point2d ptL = CameraCalculator.GetScreenPosFromPT(camPan, camTilt, viewportHor, sizeRatio, camPan - viewportHor / 2, camTilt);
            Console.WriteLine($"L {ptL.X}, {ptL.Y} R {ptR.X}, {ptR.Y}");
            Assert.IsTrue(ptL.X < 0.1);
            Assert.IsTrue(ptL.X > 0);
            Assert.IsTrue(ptL.Y > 0.5);
            Assert.IsTrue(ptL.Y < 0.6);
            Assert.IsTrue(ptR.X < 1);
            Assert.IsTrue(ptR.X > 0.9);
            Assert.IsTrue(ptR.Y > 0.5);
            Assert.IsTrue(ptR.Y < 0.6);
            Assert.AreEqual(ptL.Y, ptR.Y);
            Assert.AreEqual(1, ptL.X + ptR.X);
        }

        [TestMethod]
        public void TestGetScreenPosFromPTDown()//下侧点
        {
            Point2d pt = CameraCalculator.GetScreenPosFromPT(camPan, camTilt, viewportHor, sizeRatio, camPan, camTilt + viewportVer / 2);
            Console.WriteLine($"{pt.X}, {pt.Y}");
            Assert.AreEqual(0.5, pt.X);
            Assert.AreEqual(1, Math.Round(pt.Y, 3));
        }

        [TestMethod]
        public void TestGetScreenPosFromCamDown()//摄像机朝下方
        {
            camTilt = 80;
            Setup();
            Point2d pt = CameraCalculator.GetScreenPosFromPT(camPan, camTilt, viewportHor, sizeRatio, camPan + 180, 85);
            Console.WriteLine($"{pt.X}, {pt.Y}");
            Assert.AreEqual(0.5, pt.X);
            Assert.IsTrue(pt.Y < 1);
            Assert.IsTrue(pt.Y > 0.9);
        }

        [TestMethod]
        public void TestGetScreenPosFromCamMore()//摄像机朝下方
        {
            camPan = 113.8;
            camTilt = -0.058;
            Setup();
            Point2d pt = CameraCalculator.GetScreenPosFromPT(113.8, -0.058, 28.87, 16.0/9, 280, 1.21);
            Console.WriteLine($"{pt.X}, {pt.Y}");
            Point2d pt1 = CameraCalculator.GetScreenPosFromPT(113.8, -0.058, 28.87, 16.0 / 9, 100, 1.21);
            Console.WriteLine($"{pt1.X}, {pt1.Y}");
        }


        [TestMethod]
        public void TestGetPosInVideo()
        {
            camTilt = 5;
            Setup();
            CameraCalculator calc = new CameraCalculator(new PTZPosition(121, 30, 30, camPan, camTilt, viewportHor, sizeRatio));
            double dis = 30 / Math.Tan(camTilt * Math.PI / 180);
            //中心点距离
            double disX = dis * Math.Sin(camPan * Math.PI / 180);//322m
            double disY = dis * Math.Cos(camPan * Math.PI / 180);//117m
            {//中心点坐标
                double targetLon = 121 + disX / 1852 / 60 / Math.Cos(30 * Math.PI / 180);
                double targetLat = 30 + disY / 1852 / 60;
                var pt = calc.GetPosInVideo(targetLon, targetLat, 0);
                Console.WriteLine("Point[{0}, {1}], Dis[{2}, {3}] Cam[{4}, {5}, {6}] Target[{7},{8}]", pt.X, pt.Y, disX, disY, camPan, camTilt, viewportHor, targetLon, targetLat);
                Assert.AreEqual(0.5, Math.Round(pt.X, 4));
                Assert.AreEqual(0.5, Math.Round(pt.Y, 4));
            }
            {//往东100m
                disX += 100;
                double targetLon = 121 + disX / 1852 / 60 / Math.Cos(30 * Math.PI / 180);
                double targetLat = 30 + disY / 1852 / 60;
                var pt = calc.GetPosInVideo(targetLon, targetLat, 0);
                Console.WriteLine("Point[{0}, {1}], Dis[{2}, {3}] Cam[{4}, {5}, {6}] Target[{7},{8}]", pt.X, pt.Y, disX, disY, camPan, camTilt, viewportHor, targetLon, targetLat);
                Assert.IsTrue(pt.X > 0.5);
                Assert.IsTrue(pt.X < 0.6);
                Assert.IsTrue(pt.Y < 0.5);
                Assert.IsTrue(pt.Y > 0.4);
            }

            {//往南200m,超出图像范围
                disY -= 200;
                double targetLon = 121 + disX / 1852 / 60 / Math.Cos(30 * Math.PI / 180);
                double targetLat = 30 + disY / 1852 / 60;
                var pt = calc.GetPosInVideo(targetLon, targetLat, 0);
                Console.WriteLine("Point[{0}, {1}], Dis[{2}, {3}] Cam[{4}, {5}, {6}] Target[{7},{8}]", pt.X, pt.Y, disX, disY, camPan, camTilt, viewportHor, targetLon, targetLat);
                Assert.IsTrue(pt.X > 1);
                Assert.IsTrue(pt.X < 1.1);
                Assert.IsTrue(pt.Y < 0.5);
                Assert.IsTrue(pt.Y > 0.4);
            }
        }

        [TestMethod]
        public void TestCameraCalculator_GetPosInVideoMore()
        {
            CameraCalculator calc = new CameraCalculator(new PTZPosition(117.747439, 38.982336, 30, 113.8, -0.0582590000000067, 28.87260459, 16.0 / 9));
            var pt = calc.GetPosInVideo(117.731345, 38.98463, 0);
            Console.WriteLine(pt.X + " " + pt.Y);
        }
    }
}
