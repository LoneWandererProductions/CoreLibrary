/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Tests
 * FILE:        DocumentTests.cs
 * PURPOSE:     Document, layers and layer stack rules.
 */

using System.Collections.Immutable;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Imaging.Objects.Documents;
using Imaging.Objects.Shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Imaging.Objects.Tests
{
    [TestClass]
    public sealed class DocumentTests
    {
        /// <summary>
        /// Creates new document has no layers.
        /// </summary>
        [TestMethod]
        public void NewDocument_HasNoLayers()
        {
            using var document = new Document(10, 5);

            Assert.AreEqual(10, document.Width);
            Assert.AreEqual(5, document.Height);
            Assert.AreEqual(0, document.Layers.Count);
        }

        /// <summary>
        /// Invalids the sizes are rejected.
        /// </summary>
        [TestMethod]
        public void InvalidSizes_AreRejected()
        {
            TestSupport.Throws<ArgumentOutOfRangeException>(() => new Document(0, 10));
            TestSupport.Throws<ArgumentOutOfRangeException>(() => new Document(10, -1));

            // 40000 * 40000 * 4 bytes does not fit the int based UnmanagedImageBuffer.
            TestSupport.Throws<ArgumentOutOfRangeException>(() => new Document(40000, 40000));
        }

        /// <summary>
        /// Inserts the layer keeps bottom to top order.
        /// </summary>
        [TestMethod]
        public void InsertLayer_KeepsBottomToTopOrder()
        {
            using var document = new Document(4, 4);
            var bottom = new RasterLayer(4, 4, "bottom");
            var top = new ShapeLayer("top");
            var middle = new RasterLayer(4, 4, "middle");

            document.InsertLayer(0, bottom);
            document.InsertLayer(1, top);
            document.InsertLayer(1, middle);

            Assert.AreEqual(3, document.Layers.Count);
            Assert.AreSame(bottom, document.Layers[0]);
            Assert.AreSame(middle, document.Layers[1]);
            Assert.AreSame(top, document.Layers[2]);
            Assert.AreEqual(1, document.IndexOf(middle.Id));
        }

        /// <summary>
        /// Inserts the layer rejects wrong sized raster layer.
        /// </summary>
        [TestMethod]
        public void InsertLayer_RejectsWrongSizedRasterLayer()
        {
            using var document = new Document(4, 4);
            using var wrong = new RasterLayer(5, 4);

            TestSupport.Throws<ArgumentException>(() => document.InsertLayer(0, wrong));
            Assert.AreEqual(0, document.Layers.Count);
        }

        /// <summary>
        /// Inserts the index of the layer rejects duplicate identifier and bad.
        /// </summary>
        [TestMethod]
        public void InsertLayer_RejectsDuplicateIdAndBadIndex()
        {
            using var document = new Document(4, 4);
            var layer = new ShapeLayer("a");
            document.InsertLayer(0, layer);

            TestSupport.Throws<ArgumentException>(() => document.InsertLayer(1, layer));
            TestSupport.Throws<ArgumentOutOfRangeException>(() => document.InsertLayer(5, new ShapeLayer("b")));
            TestSupport.Throws<ArgumentOutOfRangeException>(() => document.InsertLayer(-1, new ShapeLayer("c")));
        }

        /// <summary>
        /// Removes the layer returns layer and index and unknown identifier throws.
        /// </summary>
        [TestMethod]
        public void RemoveLayer_ReturnsLayerAndIndex_AndUnknownIdThrows()
        {
            using var document = new Document(4, 4);
            var a = new ShapeLayer("a");
            var b = new ShapeLayer("b");
            document.InsertLayer(0, a);
            document.InsertLayer(1, b);

            var removed = document.RemoveLayer(a.Id, out var index);

            Assert.AreSame(a, removed);
            Assert.AreEqual(0, index);
            Assert.AreEqual(1, document.Layers.Count);
            TestSupport.Throws<KeyNotFoundException>(() => document.RemoveLayer(Guid.NewGuid(), out _));
        }

        /// <summary>
        /// Moves the layer changes order.
        /// </summary>
        [TestMethod]
        public void MoveLayer_ChangesOrder()
        {
            using var document = new Document(4, 4);
            var a = new ShapeLayer("a");
            var b = new ShapeLayer("b");
            var c = new ShapeLayer("c");
            document.InsertLayer(0, a);
            document.InsertLayer(1, b);
            document.InsertLayer(2, c);

            document.MoveLayer(a.Id, 2);

            Assert.AreSame(b, document.Layers[0]);
            Assert.AreSame(c, document.Layers[1]);
            Assert.AreSame(a, document.Layers[2]);
            TestSupport.Throws<ArgumentOutOfRangeException>(() => document.MoveLayer(a.Id, 3));
        }

        /// <summary>
        /// Changeds the is raised for structural changes.
        /// </summary>
        [TestMethod]
        public void Changed_IsRaisedForStructuralChanges()
        {
            using var document = new Document(4, 4);
            var kinds = new List<DocumentChangeKind>();
            document.Changed += (_, e) => kinds.Add(e.Kind);

            var layer = new ShapeLayer("a");
            document.InsertLayer(0, layer);
            document.InsertLayer(1, new ShapeLayer("b"));
            document.MoveLayer(layer.Id, 1);
            document.RemoveLayer(layer.Id, out _);

            CollectionAssert.AreEqual(
                new[]
                {
                    DocumentChangeKind.LayerAdded, DocumentChangeKind.LayerAdded, DocumentChangeKind.LayerMoved,
                    DocumentChangeKind.LayerRemoved
                },
                kinds);
        }

        /// <summary>
        /// Froms the buffer creates background layer and takes ownership.
        /// </summary>
        [TestMethod]
        public void FromBuffer_CreatesBackgroundLayerAndTakesOwnership()
        {
            var buffer = new UnmanagedImageBuffer(3, 2);
            buffer.Clear(1, 2, 3, 4);

            using var document = Document.FromBuffer(buffer);

            Assert.AreEqual(3, document.Width);
            Assert.AreEqual(2, document.Height);
            Assert.AreEqual(1, document.Layers.Count);
            var background = (RasterLayer)document.Layers[0];
            Assert.AreEqual("Background", background.Name);
            Assert.AreSame(buffer, background.Pixels);
        }

        /// <summary>
        /// Froms the bitmap copies pixels.
        /// </summary>
        [TestMethod]
        public void FromBitmap_CopiesPixels()
        {
            using var bitmap = new Bitmap(4, 3, PixelFormat.Format32bppArgb);
            var bytes = new byte[4 * 3 * 4];
            bytes[0] = 10; // B
            bytes[1] = 20; // G
            bytes[2] = 30; // R
            bytes[3] = 40; // A

            var data = bitmap.LockBits(new Rectangle(0, 0, 4, 3), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);
            bitmap.UnlockBits(data);

            using var document = Document.FromBitmap(bitmap);

            var layer = (RasterLayer)document.Layers[0];
            TestSupport.AssertColor(TestSupport.Pixel(layer, 0, 0), (30, 20, 10, 40), 0);
        }

        /// <summary>
        /// Disposes the releases raster layers.
        /// </summary>
        [TestMethod]
        public void Dispose_ReleasesRasterLayers()
        {
            var document = new Document(4, 4);
            var raster = new RasterLayer(4, 4);
            document.InsertLayer(0, raster);

            document.Dispose();

            Assert.IsTrue(raster.IsDisposed);
            Assert.AreEqual(0, document.Layers.Count);
        }

        /// <summary>
        /// Creates new rasterlayer_istransparent.
        /// </summary>
        [TestMethod]
        public void NewRasterLayer_IsTransparent()
        {
            using var layer = new RasterLayer(3, 3);

            TestSupport.AssertColor(TestSupport.Pixel(layer, 1, 1), (0, 0, 0, 0), 0);
        }

        /// <summary>
        /// Opacities the is clamped.
        /// </summary>
        [TestMethod]
        public void Opacity_IsClamped()
        {
            var layer = new ShapeLayer("a") { Opacity = 2 };

            Assert.AreEqual(1d, layer.Opacity, 0.0001);
            layer.Opacity = -1;
            Assert.AreEqual(0d, layer.Opacity, 0.0001);
            layer.Opacity = double.NaN;
            Assert.AreEqual(1d, layer.Opacity, 0.0001);
        }

        [TestMethod]
        public void Duplicate_CreatesIndependentRasterLayerWithNewId()
        {
            using var original = new RasterLayer(4, 4, "photo") { Opacity = 0.5, Visible = false };
            TestSupport.Fill(original, new PixelRect(0, 0, 2, 2), 255, 0, 0);

            using var copy = (RasterLayer)original.Duplicate();
            TestSupport.Fill(original, new PixelRect(0, 0, 2, 2), 0, 255, 0);

            Assert.AreNotEqual(original.Id, copy.Id);
            Assert.AreEqual("photo copy", copy.Name);
            Assert.AreEqual(0.5, copy.Opacity, 0.0001);
            Assert.IsFalse(copy.Visible);
            TestSupport.AssertColor(TestSupport.Pixel(copy, 0, 0), (255, 0, 0, 255), 0);
        }

        [TestMethod]
        public void Duplicate_ShapeLayerKeepsShapes()
        {
            var original = new ShapeLayer("ink");
            original.Insert(0, new RectShape(1, 2, 3, 4));

            var copy = (ShapeLayer)original.Duplicate("other");
            copy.Insert(1, new EllipseShape(0, 0, 5, 5));

            Assert.AreEqual("other", copy.Name);
            Assert.AreEqual(2, copy.Shapes.Count);
            Assert.AreEqual(1, original.Shapes.Count);
        }

        [TestMethod]
        public void ShapeLayer_RejectsDuplicateShapeIds_AndReplaceNeedsSameId()
        {
            var layer = new ShapeLayer("ink");
            var rect = new RectShape(0, 0, 1, 1);
            layer.Insert(0, rect);

            TestSupport.Throws<ArgumentException>(() => layer.Insert(1, rect));
            TestSupport.Throws<ArgumentException>(() => layer.Replace(0, new RectShape(0, 0, 2, 2)));

            var moved = (RectShape)rect.Translate(5, 5);
            layer.Replace(0, moved);
            Assert.AreEqual(5d, ((RectShape)layer.Shapes[0]).X, 0.0001);
        }

        [TestMethod]
        public void PixelRect_UnionAndIntersect()
        {
            var a = new PixelRect(0, 0, 10, 10);
            var b = new PixelRect(5, 5, 10, 10);

            Assert.AreEqual(new PixelRect(0, 0, 15, 15), a.Union(b));
            Assert.AreEqual(new PixelRect(5, 5, 5, 5), a.Intersect(b));
            Assert.IsTrue(a.Intersect(new PixelRect(20, 20, 5, 5)).IsEmpty);
            Assert.AreEqual(b, PixelRect.Empty.Union(b));
            Assert.AreEqual(new PixelRect(-1, -1, 12, 12), a.Inflate(1));
        }

        [TestMethod]
        public void ImmutableArrayShapes_AreAccepted()
        {
            var polygon = new PolygonShape(ImmutableArray.Create(new PointD(0, 0), new PointD(4, 0), new PointD(2, 3)));

            Assert.IsTrue(polygon.IsValid());
        }
    }
}
