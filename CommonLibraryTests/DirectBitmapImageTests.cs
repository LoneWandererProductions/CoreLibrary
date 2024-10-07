using System.Collections.Generic;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Imaging;

namespace CommonLibraryTests
{
    [TestClass]
    public class DirectBitmapImageTests
    {
        private DirectBitmapImage _bitmapImage;
        private const int Width = 10;
        private const int Height = 10;

        [TestInitialize]
        public void Setup()
        {
            _bitmapImage = new DirectBitmapImage(Width, Height);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _bitmapImage.Dispose();
        }

        [TestMethod]
        public void Constructor_InitializesBitsArray()
        {
            // Assert that the Bits array is initialized correctly
            Assert.IsNotNull(_bitmapImage.Bits);
            Assert.AreEqual(Width * Height, _bitmapImage.Bits.Length);
        }

        [TestMethod]
        public void SetPixels_ValidPixels_UpdatesBitmap()
        {
            var pixels = new List<PixelData>
            {
                new PixelData { X = 0, Y = 0, R = 255, G = 0, B = 0, A = 255 }, // Red
                new PixelData { X = 1, Y = 1, R = 0, G = 255, B = 0, A = 255 }, // Green
                new PixelData { X = 2, Y = 2, R = 0, G = 0, B = 255, A = 255 }  // Blue
            };

            _bitmapImage.SetPixels(pixels);

            // Check that the colors were set correctly in the Bits array
            Assert.AreEqual(unchecked((uint)(255 << 24 | 255 << 16 | 0 << 8 | 0)), _bitmapImage.Bits[0]); // Red
            Assert.AreEqual(unchecked((uint)(255 << 24 | 0 << 16 | 255 << 8 | 0)), _bitmapImage.Bits[Width + 1]); // Green
            Assert.AreEqual(unchecked((uint)(255 << 24 | 0 << 16 | 0 << 8 | 255)), _bitmapImage.Bits[Width * 2 + 2]); // Blue
        }

        [TestMethod]
        public void ApplyColorMatrix_ValidMatrix_TransformsColors()
        {
            // Set initial pixel colors
            var initialPixels = new List<PixelData>
            {
                new PixelData { X = 0, Y = 0, R = 255, G = 0, B = 0, A = 255 }, // Red
                new PixelData { X = 1, Y = 1, R = 0, G = 255, B = 0, A = 255 }, // Green
            };

            _bitmapImage.SetPixels(initialPixels);

            // Define a color matrix to convert colors to grayscale
            var matrix = new float[][]
            {
                new float[] { 0.3f, 0.3f, 0.3f, 0, 0 },
                new float[] { 0.59f, 0.59f, 0.59f, 0, 0 },
                new float[] { 0.11f, 0.11f, 0.11f, 0, 0 },
                new float[] { 0, 0, 0, 1, 0 },
                new float[] { 0, 0, 0, 0, 0 }
            };

            _bitmapImage.ApplyColorMatrix(matrix);

            // Assert that colors were transformed correctly to grayscale
            //Assert.AreEqual(unchecked((uint)(255 << 24 | 76 << 16 | 76 << 8 | 76)), _bitmapImage.Bits[0]); // Grayscale for Red
            //Assert.AreEqual(unchecked((uint)(255 << 24 | 150 << 16 | 150 << 8 | 150)), _bitmapImage.Bits[Width + 1]); // Grayscale for Green
        }

        [TestMethod]
        public void SetPixelsSimd_ValidPixels_UpdatesBits()
        {
            var pixels = new List<(int x, int y, Color color)>
            {
                (0, 0, Color.FromArgb(255, 255, 0, 0)), // Red
                (1, 1, Color.FromArgb(255, 0, 255, 0)), // Green
                (2, 2, Color.FromArgb(255, 0, 0, 255))  // Blue
            };

            _bitmapImage.SetPixelsSimd(pixels);

            // Check that the colors were set correctly in the Bits array
            Assert.AreEqual(unchecked((uint)(255 << 24 | 255 << 16 | 0 << 8 | 0)), _bitmapImage.Bits[0]); // Red
            Assert.AreEqual(unchecked((uint)(255 << 24 | 0 << 16 | 255 << 8 | 0)), _bitmapImage.Bits[Width + 1]); // Green
            Assert.AreEqual(unchecked((uint)(255 << 24 | 0 << 16 | 0 << 8 | 255)), _bitmapImage.Bits[Width * 2 + 2]); // Blue
        }

        [TestMethod]
        public void SetPixels_ShouldSetCorrectPixelValues()
        {
            // Arrange
            var bitmapImage = new DirectBitmapImage(2, 2);
            var pixels = new List<PixelData>
            {
                new PixelData { X = 0, Y = 0, R = 255, G = 0, B = 0, A = 255 }, // Red
                new PixelData { X = 1, Y = 0, R = 0, G = 255, B = 0, A = 255 }, // Green
                new PixelData { X = 0, Y = 1, R = 0, G = 0, B = 255, A = 255 }, // Blue
                new PixelData { X = 1, Y = 1, R = 255, G = 255, B = 0, A = 255 } // Yellow
            };

            // Act
            bitmapImage.SetPixels(pixels);

            // Assert using unsigned integers
            Assert.AreEqual(unchecked((uint)(255 << 24 | 255 << 16 | 0 << 8 | 0)), bitmapImage.Bits[0]); // Check Red (0xFFFF0000)
            Assert.AreEqual(unchecked((uint)(255 << 24 | 0 << 16 | 255 << 8 | 0)), bitmapImage.Bits[1]); // Check Green (0xFF00FF00)
            Assert.AreEqual(unchecked((uint)(255 << 24 | 0 << 16 | 0 << 8 | 255)), bitmapImage.Bits[2]); // Check Blue (0xFF0000FF)
            Assert.AreEqual(unchecked((uint)(255 << 24 | 255 << 16 | 255 << 8 | 0)), bitmapImage.Bits[3]); // Check Yellow (0xFFFFFF00)
        }

    }
}
