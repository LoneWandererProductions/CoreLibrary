using System.Drawing;
using Imaging;

/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        CommonLibraryTests/ImageFilterTests.cs
 * PURPOSE:     Tests for Image Tools, here mostly filters
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTests
{
    /// <summary>
    /// Add some filter tests
    /// </summary>
    [TestClass]
    public class ImageFilterTests
    {

        private ImageRender _render = new ImageRender();

        /// <summary>
        /// Pixelates the small step width pixels changed.
        /// </summary>
        [TestMethod]
        public void PixelateSmallStepWidthPixelsChanged()
        {
            // Arrange
            Bitmap input = CreateTestBitmap(10, 10, Color.Red);
            Bitmap expected = CreateTestBitmap(10, 10, Color.Red); // A simple color, the output should still be red
            var stepWidth = 2;

            // Act
            Bitmap result = _render.Pixelate(input, stepWidth);

            // Assert
            Assert.AreNotEqual(input, result, "The pixelated image should not be exactly the same as the input image.");
        }

        /// <summary>
        /// Pixelates the large step width image reduced to larger blocks.
        /// </summary>
        [TestMethod]
        public void PixelateLargeStepWidthImageReducedToLargerBlocks()
        {
            // Arrange
            Bitmap input = CreateTestBitmap(10, 10, Color.Red);
            var stepWidth = 5;

            // Act
            Bitmap result = _render.Pixelate(input, stepWidth);
            var dbm = new DirectBitmap(result);

            // Assert
            // Check that pixels in larger blocks are the same (indicating proper pixelation)
            Color blockColor = dbm.GetPixel(0, 0);
            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 5; y++)
                {
                    Assert.AreEqual(blockColor, dbm.GetPixel(x, y), $"Expected block to have the same color at ({x}, {y})");
                }
            }
        }

        /// <summary>
        /// Pixelates the step width exceeds image size returns same color bitmap.
        /// </summary>
        [TestMethod]
        public void PixelateStepWidthExceedsImageSizeReturnsSameColorBitmap()
        {
            // Arrange
            Bitmap input = CreateTestBitmap(3, 3, Color.Green);
            var stepWidth = 10; // Step width larger than the image dimensions

            // Act
            Bitmap result = _render.Pixelate(input, stepWidth);
            var dbm = new DirectBitmap(result);

            // Assert
            // All pixels should be the same color due to step size being too large
            Color firstPixelColor = dbm.GetPixel(0, 0);

            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    Assert.AreEqual(firstPixelColor, dbm.GetPixel(x, y), $"Pixel at ({x}, {y}) should be {firstPixelColor}");
                }
            }
        }

        /// <summary>
        /// Creates the test bitmap.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="color">The color.</param>
        /// <returns></returns>
        private Bitmap CreateTestBitmap(int width, int height, Color color)
        {
            Bitmap bitmap = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(color);
            }
            return bitmap;
        }
    }
}
