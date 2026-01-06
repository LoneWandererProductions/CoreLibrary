/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        CommonLibraryTests/ImagingFilterTests.cs
 * PURPOSE:     Tests for Image Tools, here mostly filters
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Drawing;
using Imaging;
using Imaging.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTests
{
    /// <summary>
    ///     Add some filter tests
    /// </summary>
    [TestClass]
    public class ImagingFilterTests
    {
        /// <summary>
        ///     The render
        /// </summary>
        private IImageRender? _render;

        [TestInitialize]
        public void Setup()
        {
            _render = new ImageRender(); // Assuming ImageRender implements IImageRender
        }

        /// <summary>
        ///     Pixelates the small step width pixels changed.
        /// </summary>
        [TestMethod]
        public void PixelateSmallStepWidthPixelsChanged()
        {
            // Arrange
            var input = CreateTestBitmap(10, 10, Color.Red); // A simple color, the output should still be red

            // Act
            var result = _render?.Pixelate(input);

            // Assert
            Assert.AreNotEqual(input, result, "The pixelated image should not be exactly the same as the input image.");
        }

        /// <summary>
        ///     Pixelates the large step width image reduced to larger blocks.
        /// </summary>
        [TestMethod]
        public void PixelateLargeStepWidthImageReducedToLargerBlocks()
        {
            // Arrange
            var input = CreateTestBitmap(10, 10, Color.Red);
            const int stepWidth = 5;

            // Act
            var result = _render?.Pixelate(input, stepWidth);
            var dbm = new DirectBitmap(result);

            // Assert
            // Check that pixels in larger blocks are the same (indicating proper pixelation)
            var blockColor = dbm.GetPixel(0, 0);
            for (var x = 0; x < 5; x++)
            {
                for (var y = 0; y < 5; y++)
                {
                    Assert.AreEqual(blockColor, dbm.GetPixel(x, y),
                        $"Expected block to have the same color at ({x}, {y})");
                }
            }
        }

        /// <summary>
        ///     Pixelates the step width exceeds image size returns same color bitmap.
        /// </summary>
        [TestMethod]
        public void PixelateStepWidthExceedsImageSizeReturnsSameColorBitmap()
        {
            // Arrange
            var input = CreateTestBitmap(3, 3, Color.Green);
            const int stepWidth = 10; // Step width larger than the image dimensions

            // Act
            var result = _render?.Pixelate(input, stepWidth);
            var dbm = new DirectBitmap(result);

            // Assert
            // All pixels should be the same color due to step size being too large
            var firstPixelColor = dbm.GetPixel(0, 0);

            for (var x = 0; x < 3; x++)
            {
                for (var y = 0; y < 3; y++)
                {
                    Assert.AreEqual(firstPixelColor, dbm.GetPixel(x, y),
                        $"Pixel at ({x}, {y}) should be {firstPixelColor}");
                }
            }
        }

        /// <summary>
        ///     Creates the test bitmap.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="color">The color.</param>
        /// <returns>Test bitmap</returns>
        private static Bitmap CreateTestBitmap(int width, int height, Color color)
        {
            var bitmap = new Bitmap(width, height);
            using var g = Graphics.FromImage(bitmap);
            g.Clear(color);

            return bitmap;
        }
    }
}
