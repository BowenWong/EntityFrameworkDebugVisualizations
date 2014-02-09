using EntityFramework.Debug.DebugVisualization.Views.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EntityFramework.Debug.UnitTests.Tests
{
    [TestClass]
    public class MiniMapBehaviors
    {
        [TestMethod]
        public void TestOuterDimensionsSquare()
        {
            const double zoomControlWidth = 100.0;
            const double zoomControlHeight = 100.0;
            const double miniMapScale = 0.1;

            var width = MiniMapControl.GetWidth(zoomControlWidth, miniMapScale);
            var height = MiniMapControl.GetHeight(zoomControlHeight, miniMapScale);

            Assert.AreEqual(10.0, width);
            Assert.AreEqual(10.0, height);
        }

        [TestMethod]
        public void TestInnerDimensionsSquareBoth()
        {
            const double mapContentWidth = 60;
            const double mapContentHeight = 60;

            const double contentWidth = 100;
            const double contentHeight = 100;

            var width = MiniMapControl.GetContentWidth(mapContentWidth, mapContentHeight, contentWidth, contentHeight);
            var height = MiniMapControl.GetContentHeight(mapContentWidth, mapContentHeight, contentWidth, contentHeight);

            Assert.IsTrue(0 < width);
            Assert.IsTrue(0 < height);

            Assert.IsTrue(width <= mapContentWidth);
            Assert.IsTrue(height <= mapContentHeight);

            Assert.AreEqual(mapContentWidth, width);
            Assert.AreEqual(mapContentHeight, height);
        }

        [TestMethod]
        public void TestInnerDimensionsWidthLargerBoth()
        {
            const double mapContentWidth = 80;
            const double mapContentHeight = 60;

            const double contentWidth = 100;
            const double contentHeight = 80;

            var width = MiniMapControl.GetContentWidth(mapContentWidth, mapContentHeight, contentWidth, contentHeight);
            var height = MiniMapControl.GetContentHeight(mapContentWidth, mapContentHeight, contentWidth, contentHeight);
            
            Assert.IsTrue(0 < width);
            Assert.IsTrue(0 < height);

            Assert.IsTrue(width <= mapContentWidth);
            Assert.IsTrue(height <= mapContentHeight);

            Assert.AreEqual(75, width);
            Assert.AreEqual(mapContentHeight, height);
        }

        [TestMethod]
        public void TestInnerDimensionsHeightLargerBoth()
        {
            const double mapContentWidth = 60;
            const double mapContentHeight = 80;

            const double contentWidth = 80;
            const double contentHeight = 100;

            var width = MiniMapControl.GetContentWidth(mapContentWidth, mapContentHeight, contentWidth, contentHeight);
            var height = MiniMapControl.GetContentHeight(mapContentWidth, mapContentHeight, contentWidth, contentHeight);

            Assert.IsTrue(0 < width);
            Assert.IsTrue(0 < height);

            Assert.IsTrue(width <= mapContentWidth);
            Assert.IsTrue(height <= mapContentHeight);

            Assert.AreEqual(mapContentWidth, width);
            Assert.AreEqual(75, height);
        }

        [TestMethod]
        public void TestInnerDimensionsWidthLargerMap()
        {
            const double mapContentWidth = 80;
            const double mapContentHeight = 60;

            const double contentWidth = 100;
            const double contentHeight = 100;

            var width = MiniMapControl.GetContentWidth(mapContentWidth, mapContentHeight, contentWidth, contentHeight);
            var height = MiniMapControl.GetContentHeight(mapContentWidth, mapContentHeight, contentWidth, contentHeight);

            Assert.IsTrue(0 < width);
            Assert.IsTrue(0 < height);

            Assert.IsTrue(width <= mapContentWidth);
            Assert.IsTrue(height <= mapContentHeight);

            Assert.AreEqual(60, width);
            Assert.AreEqual(mapContentHeight, height);
        }

        [TestMethod]
        public void TestInnerDimensionsHeightLargerMap()
        {
            const double mapContentWidth = 60;
            const double mapContentHeight = 80;

            const double contentWidth = 100;
            const double contentHeight = 100;

            var width = MiniMapControl.GetContentWidth(mapContentWidth, mapContentHeight, contentWidth, contentHeight);
            var height = MiniMapControl.GetContentHeight(mapContentWidth, mapContentHeight, contentWidth, contentHeight);

            Assert.IsTrue(0 < width);
            Assert.IsTrue(0 < height);

            Assert.IsTrue(width <= mapContentWidth);
            Assert.IsTrue(height <= mapContentHeight);

            Assert.AreEqual(mapContentWidth, width);
            Assert.AreEqual(60, height);
        }

        [TestMethod]
        public void TestInnerDimensionsWidthLargerContent()
        {
            const double mapContentWidth = 60;
            const double mapContentHeight = 60;

            const double contentWidth = 100;
            const double contentHeight = 80;

            var width = MiniMapControl.GetContentWidth(mapContentWidth, mapContentHeight, contentWidth, contentHeight);
            var height = MiniMapControl.GetContentHeight(mapContentWidth, mapContentHeight, contentWidth, contentHeight);

            Assert.IsTrue(0 < width);
            Assert.IsTrue(0 < height);

            Assert.IsTrue(width <= mapContentWidth);
            Assert.IsTrue(height <= mapContentHeight);

            Assert.AreEqual(mapContentWidth, width);
            Assert.AreEqual(48, height);
        }

        [TestMethod]
        public void TestInnerDimensionsHeightLargerContent()
        {
            const double mapContentWidth = 60;
            const double mapContentHeight = 60;

            const double contentWidth = 80;
            const double contentHeight = 100;

            var width = MiniMapControl.GetContentWidth(mapContentWidth, mapContentHeight, contentWidth, contentHeight);
            var height = MiniMapControl.GetContentHeight(mapContentWidth, mapContentHeight, contentWidth, contentHeight);

            Assert.IsTrue(0 < width);
            Assert.IsTrue(0 < height);

            Assert.IsTrue(width <= mapContentWidth);
            Assert.IsTrue(height <= mapContentHeight);

            Assert.AreEqual(48, width);
            Assert.AreEqual(mapContentHeight, height);
        }

        [TestMethod]
        public void TestInnerDimensionsContentHigherMapWider()
        {
            const double mapContentWidth = 100;
            const double mapContentHeight = 60;

            const double contentWidth = 60;
            const double contentHeight = 100;

            var width = MiniMapControl.GetContentWidth(mapContentWidth, mapContentHeight, contentWidth, contentHeight);
            var height = MiniMapControl.GetContentHeight(mapContentWidth, mapContentHeight, contentWidth, contentHeight);

            Assert.IsTrue(0 < width);
            Assert.IsTrue(0 < height);

            Assert.IsTrue(width <= mapContentWidth);
            Assert.IsTrue(height <= mapContentHeight);

            Assert.AreEqual(36, width);
            Assert.AreEqual(mapContentHeight, height);
        }

        [TestMethod]
        public void TestInnerDimensionsContentWiderMapHigher()
        {
            const double mapContentWidth = 60;
            const double mapContentHeight = 100;

            const double contentWidth = 100;
            const double contentHeight = 60;

            var width = MiniMapControl.GetContentWidth(mapContentWidth, mapContentHeight, contentWidth, contentHeight);
            var height = MiniMapControl.GetContentHeight(mapContentWidth, mapContentHeight, contentWidth, contentHeight);

            Assert.IsTrue(0 < width);
            Assert.IsTrue(0 < height);

            Assert.IsTrue(width <= mapContentWidth);
            Assert.IsTrue(height <= mapContentHeight);

            Assert.AreEqual(mapContentWidth, width);
            Assert.AreEqual(36, height);
        }

        [TestMethod]
        public void TestInnerDimensionsContentExtremeWide()
        {
            const double mapContentWidth = 100;
            const double mapContentHeight = 60;

            const double contentWidth = 1000;
            const double contentHeight = 250;

            var width = MiniMapControl.GetContentWidth(mapContentWidth, mapContentHeight, contentWidth, contentHeight);
            var height = MiniMapControl.GetContentHeight(mapContentWidth, mapContentHeight, contentWidth, contentHeight);

            Assert.IsTrue(0 < width);
            Assert.IsTrue(0 < height);

            Assert.IsTrue(width <= mapContentWidth);
            Assert.IsTrue(height <= mapContentHeight);

            Assert.AreEqual(mapContentWidth, width);
            Assert.AreEqual(mapContentHeight, height);
        }

        [TestMethod]
        public void TestInnerDimensionsContentExtremeHigh()
        {
            const double mapContentWidth = 60;
            const double mapContentHeight = 100;

            const double contentWidth = 250;
            const double contentHeight = 1000;

            var width = MiniMapControl.GetContentWidth(mapContentWidth, mapContentHeight, contentWidth, contentHeight);
            var height = MiniMapControl.GetContentHeight(mapContentWidth, mapContentHeight, contentWidth, contentHeight);

            Assert.IsTrue(0 < width);
            Assert.IsTrue(0 < height);

            Assert.IsTrue(width <= mapContentWidth);
            Assert.IsTrue(height <= mapContentHeight);

            Assert.AreEqual(mapContentWidth, width);
            Assert.AreEqual(mapContentHeight, height);
        }
    }
}