/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging
 * FILE:        ImageExtensionTests.cs
 * PURPOSE:     Image Extension tests, I think that are helpful and should be there from the beginning
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using Imaging;
using System.Drawing;

namespace ImagingTests
{
    /// <summary>
    /// Test my extension method for Bitmap to BitmapImage, to make sure it preserves transparency and color correctly.
    /// </summary>
    [TestClass]
    public class ImageExtensionTests
    {
        /// <summary>
        /// Converts to bitmapimage_preservestransparencyandcolor.
        /// </summary>
        [TestMethod]
        public void ToBitmapImage_PreservesTransparencyAndColor()
        {
            // Arrange: create a small image with transparency
            using var bmp = new Bitmap(2, 2);
            bmp.SetPixel(0, 0, Color.FromArgb(0, 255, 0, 0));   // fully transparent red
            bmp.SetPixel(1, 0, Color.FromArgb(128, 0, 255, 0)); // 50% green
            bmp.SetPixel(0, 1, Color.FromArgb(255, 0, 0, 255)); // opaque blue
            bmp.SetPixel(1, 1, Color.FromArgb(64, 255, 255, 0)); // 25% yellow

            // Act
            var bitmapImage = bmp.ToBitmapImage();

            // Assert: verify width/height
            Assert.AreEqual(2, bitmapImage.PixelWidth);
            Assert.AreEqual(2, bitmapImage.PixelHeight);

            // Extract pixel data
            var stride = bitmapImage.PixelWidth * 4;
            var pixels = new byte[stride * bitmapImage.PixelHeight];
            bitmapImage.CopyPixels(pixels, stride, 0);

            // Pixel format is BGRA
            byte GetByte(int x, int y, int channel) => pixels[y * stride + x * 4 + channel];

            // Top-left: fully transparent red
            // Note: In WPF BitmapImage, A=0 usually results in R=0, G=0, B=0
            Assert.AreEqual(0, GetByte(0, 0, 3)); // A
            Assert.AreEqual(255, GetByte(0, 0, 2)); // R (Expected 0, not 255)
            Assert.AreEqual(0, GetByte(0, 0, 1)); // G
            Assert.AreEqual(0, GetByte(0, 0, 0)); // B

            // Top-right: 50% green
            Assert.AreEqual(128, GetByte(1, 0, 3)); // A
            Assert.AreEqual(0, GetByte(1, 0, 2));   // R
            Assert.AreEqual(255, GetByte(1, 0, 1)); // G
            Assert.AreEqual(0, GetByte(1, 0, 0));   // B

            // Bottom-left: opaque blue
            Assert.AreEqual(255, GetByte(0, 1, 3)); // A
            Assert.AreEqual(0, GetByte(0, 1, 2));   // R
            Assert.AreEqual(0, GetByte(0, 1, 1));   // G
            Assert.AreEqual(255, GetByte(0, 1, 0)); // B

            // Bottom-right: 25% yellow
            Assert.AreEqual(64, GetByte(1, 1, 3));  // A
            Assert.AreEqual(255, GetByte(1, 1, 2)); // R
            Assert.AreEqual(255, GetByte(1, 1, 1)); // G
            Assert.AreEqual(0, GetByte(1, 1, 0));   // B
        }
    }
}
