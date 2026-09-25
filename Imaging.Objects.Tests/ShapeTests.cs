/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Tests
 * FILE:        ShapeTests.cs
 * PURPOSE:     Shapes as data: bounds, translation, validation, JSON.
 */

using System.Collections.Immutable;
using System.Text.Json;
using Imaging.Objects.Documents;
using Imaging.Objects.Shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Imaging.Objects.Tests
{
    [TestClass]
    public sealed class ShapeTests
    {
        private static ImmutableArray<PointD> Triangle =>
            ImmutableArray.Create(new PointD(2, 2), new PointD(12, 4), new PointD(6, 9));

        [TestMethod]
        public void RectBounds_IncludeStrokeAndAntiAliasing()
        {
            var rect = new RectShape(10, 10, 20, 5) { Stroke = new StrokeSpec(0xFF000000, 4) };

            // half the stroke (2) + 1 px anti-aliasing = 3 on every side
            Assert.AreEqual(new PixelRect(7, 7, 26, 11), rect.GetBounds());
        }

        [TestMethod]
        public void LineBounds_DoNotDependOnPointOrder()
        {
            var a = new LineShape(new PointD(5, 8), new PointD(1, 2));
            var b = new LineShape(new PointD(1, 2), new PointD(5, 8));

            Assert.AreEqual(a.GetBounds(), b.GetBounds());
            Assert.AreEqual(new PixelRect(-1, 0, 8, 10), a.GetBounds()); // default stroke 1 -> pad 2 (ceil(0.5)+1)
        }

        [TestMethod]
        public void PolygonBounds_CoverAllPoints()
        {
            var bounds = new PolygonShape(Triangle) { Stroke = new StrokeSpec(0xFF000000, 0) }.GetBounds();

            Assert.IsTrue(bounds.X <= 2 && bounds.Y <= 2 && bounds.Right >= 12 && bounds.Bottom >= 9);
        }

        [TestMethod]
        public void Translate_KeepsIdAndStyle_AndMovesGeometry()
        {
            var fill = new SolidFill(0xFFFF0000);
            Shape[] shapes =
            {
                new LineShape(new PointD(0, 0), new PointD(1, 1)) { Fill = fill },
                new RectShape(0, 0, 4, 4) { Fill = fill }, new EllipseShape(0, 0, 4, 4) { Fill = fill },
                new PolygonShape(Triangle) { Fill = fill }, new PolylineShape(Triangle) { Fill = fill }
            };

            foreach (var shape in shapes)
            {
                var moved = shape.Translate(10, -3);

                Assert.AreEqual(shape.Id, moved.Id);
                Assert.AreEqual(shape.Fill, moved.Fill);
                Assert.AreEqual(shape.Stroke, moved.Stroke);
                Assert.AreEqual(shape.GetBounds().X + 10, moved.GetBounds().X);
                Assert.AreEqual(shape.GetBounds().Y - 3, moved.GetBounds().Y);
                Assert.AreEqual(shape.GetBounds().Width, moved.GetBounds().Width);
            }
        }

        [TestMethod]
        public void Translate_DoesNotChangeTheOriginal()
        {
            var polygon = new PolygonShape(Triangle);

            polygon.Translate(100, 100);

            Assert.AreEqual(2d, polygon.Points[0].X, 0.0001);
        }

        [TestMethod]
        public void IsValid_RejectsBrokenShapes()
        {
            Assert.IsTrue(new RectShape(0, 0, 1, 1).IsValid());
            Assert.IsFalse(new RectShape(0, 0, -1, 1).IsValid());
            Assert.IsFalse(new RectShape(double.NaN, 0, 1, 1).IsValid());
            Assert.IsFalse(new EllipseShape(0, 0, double.PositiveInfinity, 1).IsValid());
            Assert.IsFalse(new LineShape(new PointD(0, 0), new PointD(double.NaN, 1)).IsValid());
            Assert.IsFalse(new RectShape(0, 0, 1, 1) { Stroke = new StrokeSpec(0, -1) }.IsValid());
            Assert.IsFalse(new PolygonShape(ImmutableArray.Create(new PointD(0, 0), new PointD(1, 1))).IsValid());
            Assert.IsFalse(new PolylineShape(ImmutableArray.Create(new PointD(0, 0))).IsValid());
            Assert.IsFalse(new PolygonShape(default).IsValid());
        }

        [TestMethod]
        public void Json_RoundTripsEveryShapeAndFillKind()
        {
            Shape[] shapes =
            {
                new LineShape(new PointD(1.5, 2.5), new PointD(10, 20))
                {
                    Stroke = new StrokeSpec(0x80FF0000, 3.5)
                },
                new RectShape(1, 2, 3, 4) { Fill = new SolidFill(0xFF00FF00) },
                new EllipseShape(5, 6, 7, 8) { Fill = new TextureFill("Cloud") },
                new PolygonShape(Triangle) { Fill = new FilterFill("Sepia") }, new PolylineShape(Triangle)
            };

            foreach (var shape in shapes)
            {
                var json = JsonSerializer.Serialize(shape);
                var back = JsonSerializer.Deserialize<Shape>(json);

                Assert.IsNotNull(back);
                Assert.AreEqual(shape.GetType(), back!.GetType());
                Assert.AreEqual(shape.Id, back.Id);
                Assert.AreEqual(shape.Stroke, back.Stroke);
                Assert.AreEqual(shape.Fill, back.Fill);
                Assert.AreEqual(json, JsonSerializer.Serialize(back));
                Assert.IsTrue(back.IsValid());
            }
        }

        [TestMethod]
        public void Json_UsesReadableTypeDiscriminators()
        {
            var json = JsonSerializer.Serialize<Shape>(new RectShape(0, 0, 1, 1) { Fill = new SolidFill(1) });

            Assert.IsTrue(json.Contains("\"type\":\"rect\""), json);
            Assert.IsTrue(json.Contains("\"kind\":\"solid\""), json);
        }
    }
}
