/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Tests
 * FILE:        FlattenTests.cs
 * PURPOSE:     Compositing of layers, and that LayeredImageContainer still behaves as before after sharing its blend loop.
 */

using Imaging.Objects.Documents;
using Imaging.Objects.Shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Imaging.Objects.Tests
{
    [TestClass]
    public sealed class FlattenTests
    {
        private static RasterLayer Solid(int size, byte r, byte g, byte b, byte a, string name = "layer")
        {
            var layer = new RasterLayer(size, size, name);
            layer.Pixels.Clear(r, g, b, a);
            return layer;
        }

        [TestMethod]
        public void EmptyDocument_FlattensToTransparent()
        {
            using var document = new Document(4, 4);
            using var result = DocumentRenderer.Flatten(document);

            Assert.AreEqual(4, result.Width);
            TestSupport.AssertColor(TestSupport.Pixel(result, 2, 2), (0, 0, 0, 0), 0);
        }

        [TestMethod]
        public void OpaqueLayerOnTop_Wins()
        {
            using var document = new Document(4, 4);
            document.InsertLayer(0, Solid(4, 255, 0, 0, 255));
            document.InsertLayer(1, Solid(4, 0, 255, 0, 255));

            using var result = DocumentRenderer.Flatten(document);

            TestSupport.AssertColor(TestSupport.Pixel(result, 1, 1), (0, 255, 0, 255), 0);
        }

        [TestMethod]
        public void HalfTransparentPixel_BlendsWithStraightAlpha()
        {
            using var document = new Document(4, 4);
            document.InsertLayer(0, Solid(4, 0, 0, 255, 255));
            document.InsertLayer(1, Solid(4, 255, 0, 0, 128));

            using var result = DocumentRenderer.Flatten(document);

            TestSupport.AssertColor(TestSupport.Pixel(result, 0, 0), (128, 0, 127, 255));
        }

        [TestMethod]
        public void LayerOpacity_ScalesTheLayerAlpha()
        {
            using var document = new Document(4, 4);
            document.InsertLayer(0, Solid(4, 0, 0, 255, 255));
            var top = Solid(4, 255, 0, 0, 255);
            top.Opacity = 0.5;
            document.InsertLayer(1, top);

            using var result = DocumentRenderer.Flatten(document);

            TestSupport.AssertColor(TestSupport.Pixel(result, 0, 0), (128, 0, 127, 255));
        }

        [TestMethod]
        public void HiddenAndFullyTransparentLayers_AreSkipped()
        {
            using var document = new Document(4, 4);
            document.InsertLayer(0, Solid(4, 0, 0, 255, 255));
            var hidden = Solid(4, 255, 0, 0, 255);
            hidden.Visible = false;
            var ghost = Solid(4, 0, 255, 0, 255);
            ghost.Opacity = 0;
            document.InsertLayer(1, hidden);
            document.InsertLayer(2, ghost);

            using var result = DocumentRenderer.Flatten(document);

            TestSupport.AssertColor(TestSupport.Pixel(result, 0, 0), (0, 0, 255, 255), 0);
        }

        [TestMethod]
        public void TransparentPixelsOfALayer_LeaveTheBackgroundAlone()
        {
            using var document = new Document(4, 4);
            document.InsertLayer(0, Solid(4, 0, 0, 255, 255));
            var top = new RasterLayer(4, 4, "top");
            top.Pixels.SetPixel(1, 1, 255, 0, 0, 255);
            document.InsertLayer(1, top);

            using var result = DocumentRenderer.Flatten(document);

            TestSupport.AssertColor(TestSupport.Pixel(result, 1, 1), (255, 0, 0, 255), 0);
            TestSupport.AssertColor(TestSupport.Pixel(result, 0, 0), (0, 0, 255, 255), 0);
        }

        [TestMethod]
        public void ShapeLayer_IsRasterizedAndCompositedInStackOrder()
        {
            using var document = new Document(10, 10);
            document.InsertLayer(0, Solid(10, 0, 0, 255, 255, "blue"));
            var shapes = new ShapeLayer("ink");
            shapes.Insert(0, new RectShape(2, 2, 4, 4) { Fill = new SolidFill(0xFFFF0000) });
            document.InsertLayer(1, shapes);
            var rasterizer = new TestSupport.FakeRasterizer();

            using var result = DocumentRenderer.Flatten(document, rasterizer);

            Assert.AreEqual(1, rasterizer.Calls);
            TestSupport.AssertColor(TestSupport.Pixel(result, 3, 3), (255, 0, 0, 255), 0);
            TestSupport.AssertColor(TestSupport.Pixel(result, 0, 0), (0, 0, 255, 255), 0);
        }

        [TestMethod]
        public void ShapeLayerWithoutRasterizer_Throws_ButEmptyOrHiddenOnesDoNot()
        {
            using var document = new Document(10, 10);
            var empty = new ShapeLayer("empty");
            var hidden = new ShapeLayer("hidden") { Visible = false };
            hidden.Insert(0, new RectShape(0, 0, 1, 1));
            document.InsertLayer(0, empty);
            document.InsertLayer(1, hidden);

            using (DocumentRenderer.Flatten(document))
            {
            }

            var real = new ShapeLayer("real");
            real.Insert(0, new RectShape(0, 0, 1, 1));
            document.InsertLayer(2, real);

            TestSupport.Throws<InvalidOperationException>(() => DocumentRenderer.Flatten(document));
        }

        [TestMethod]
        public void Flatten_DoesNotModifyTheLayers()
        {
            using var document = new Document(4, 4);
            var layer = Solid(4, 10, 20, 30, 200);
            document.InsertLayer(0, layer);
            var before = TestSupport.Snapshot(layer);

            using var result = DocumentRenderer.Flatten(document);

            Assert.IsTrue(TestSupport.SamePixels(layer, before));
        }

        [TestMethod]
        public void LayeredImageContainer_KeepsItsBlendBehaviour()
        {
            using var container = new LayeredImageContainer(2, 2);
            var bottom = new UnmanagedImageBuffer(2, 2);
            bottom.Clear(0, 0, 255, 255);
            var top = new UnmanagedImageBuffer(2, 2);
            top.Clear(0, 0, 0, 0);
            top.SetPixel(0, 0, 255, 0, 0, 128); // half transparent red
            top.SetPixel(1, 0, 0, 255, 0, 255); // opaque green
            container.AddLayer(bottom);
            container.AddLayer(top);

            using var result = container.Composite();

            TestSupport.AssertColor(result.GetPixel(0, 0), (128, 0, 127, 255));
            TestSupport.AssertColor(result.GetPixel(1, 0), (0, 255, 0, 255), 0);
            TestSupport.AssertColor(result.GetPixel(0, 1), (0, 0, 255, 255), 0);
        }
    }
}
