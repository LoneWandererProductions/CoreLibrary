/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Tests
 * FILE:        ShapeRasterizerTests.cs
 * PURPOSE:     Test our shape rasterizer.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Imaging.Enums;
using Imaging.Objects;
using Imaging.Objects.Documents;
using Imaging.Objects.Shapes;

namespace Imaging.Tests
{
    [TestClass]
    public class ShapeRasterizerTests
    {
        /// <summary>
        /// The rasterizer
        /// </summary>
        private ShapeRasterizer? _rasterizer;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            _rasterizer = new ShapeRasterizer();
        }

        /// <summary>
        /// Constructors the null render throws argument null exception.
        /// </summary>
        [TestMethod]
        public void Constructor_NullRender_ThrowsArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                new ShapeRasterizer(null, new TextureGenerator()));
        }

        /// <summary>
        /// Constructors the null texture gen throws argument null exception.
        /// </summary>
        [TestMethod]
        public void Constructor_NullTextureGen_ThrowsArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                new ShapeRasterizer(new ImageRender(), null));
        }

        /// <summary>
        /// Rasterize the null shapes throws argument null exception.
        /// </summary>
        [TestMethod]
        public void Rasterize_NullShapes_ThrowsArgumentNullException()
        {
            using var buffer = new UnmanagedImageBuffer(100, 100);
            Assert.ThrowsException<ArgumentNullException>(() =>
                _rasterizer.Rasterize(null, buffer));
        }

        /// <summary>
        /// Rasterize the null buffer throws argument null exception.
        /// </summary>
        [TestMethod]
        public void Rasterize_NullBuffer_ThrowsArgumentNullException()
        {
            var shapes = new List<Shape>();
            Assert.ThrowsException<ArgumentNullException>(() =>
                _rasterizer.Rasterize(shapes, null));
        }

        /// <summary>
        /// Rasterize the solid fill mutates buffer.
        /// </summary>
        [TestMethod]
        public void Rasterize_SolidFill_MutatesBuffer()
        {
            using var buffer = new UnmanagedImageBuffer(10, 10);
            uint whiteArgb = 0xFFFFFFFF;

            var rect = new RectShape(0, 0, 5, 5)
            {
                Fill = new SolidFill(whiteArgb), Stroke = new StrokeSpec(0, 0) // uint Argb, double Width
            };

            _rasterizer?.Rasterize(new List<Shape> { rect }, buffer);

            var insidePixel = buffer.GetPixelSpan(2, 2, 1);
            Assert.AreNotEqual(0, insidePixel[0], "SolidFill failed to mutate the buffer.");
        }

        /// <summary>
        /// Rasterize the texture fill mutates buffer with real engine.
        /// </summary>
        [TestMethod]
        public void Rasterize_TextureFill_MutatesBufferWithRealEngine()
        {
            using var buffer = new UnmanagedImageBuffer(50, 50);

            var rect = new RectShape(0, 0, 20, 20)
            {
                Fill = new TextureFill(TextureType.Marble.ToString()), Stroke = new StrokeSpec(0, 0)
            };

            _rasterizer.Rasterize(new List<Shape> { rect }, buffer);

            var insidePixel = buffer.GetPixelSpan(10, 10, 1);
            Assert.AreNotEqual(0, insidePixel[0], "TextureGenerator failed to mutate the buffer.");
        }

        /// <summary>
        /// Rasterize the filter fill executes without crashing.
        /// </summary>
        [TestMethod]
        public void Rasterize_FilterFill_ExecutesWithoutCrashing()
        {
            using var buffer = new UnmanagedImageBuffer(50, 50);

            var ellipse = new EllipseShape(10, 10, 20, 20)
            {
                Fill = new FilterFill(FiltersType.BoxBlur.ToString()), Stroke = new StrokeSpec(0, 0)
            };

            var shapes = new List<Shape> { ellipse };

            try
            {
                _rasterizer?.Rasterize(shapes, buffer);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Real ImageRender crashed during FilterFill: {ex.Message}");
            }
        }
    }
}
